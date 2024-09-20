import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload'
import { AccountService } from '../../services/account.service';
import { environment } from '../../../environments/environment';
import { MembersService } from '../../services/members.service';
import { Photo } from '../../models/photo';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgFor, NgIf, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberSErvice = inject(MembersService);
  member = input.required<Member>();
  uploader? : FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChanged = output<Member>();

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(event: any){
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader(){
    this.uploader = new FileUploader({
      url: this.baseUrl + 'user/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });
    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updatedMember = {...this.member()}
      updatedMember.photos.push(photo);
      this.memberChanged.emit(updatedMember);
      if(photo.isMain){
        const user = this.accountService.currentUser();
        if(user){
          user.photoUrl = photo.url,
          this.accountService.setCurrentUser(user)
        }
        updatedMember.photoUrl = photo.url,
        updatedMember.photos.forEach(p => {
          if(p.isMain) p.isMain = false;
          if(p.id === photo.id) p.isMain = true;
        });
        this.memberChanged.emit(updatedMember);
      }
    }
  }

  setProfilePic(photo: Photo){
    this.memberSErvice.setProfilePic(photo).subscribe({
      next: _ => {
        const user = this.accountService.currentUser();
        if(user){
          user.photoUrl = photo.url,
          this.accountService.setCurrentUser(user)
        }
        const updatedMember = {...this.member()}
        updatedMember.photoUrl = photo.url,
        updatedMember.photos.forEach(p => {
          if(p.isMain) p.isMain = false;
          if(p.id === photo.id) p.isMain = true;
        });
        this.memberChanged.emit(updatedMember);
      }
    })
  }

  deletePhoto(photo: Photo){
    this.memberSErvice.deletePhoto(photo).subscribe({
      next: _=> {
        const updatedMember = {...this.member()};
        updatedMember.photos = updatedMember.photos.filter(x => x.id != photo.id);
        this.memberChanged.emit(updatedMember);
      }
    })
  }

}
