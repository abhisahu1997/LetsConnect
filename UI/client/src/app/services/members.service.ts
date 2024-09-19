import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../models/photo';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);
  constructor() { }

  getMembers(){
    return this.http.get<Member[]>(this.baseUrl + 'user').subscribe({
      next: members => this.members.set(members)
    });
  }

  getMember(username: string){

    const member = this.members().find(x => x.username == username);
    if(member !== undefined) return of(member);

    return this.http.get<Member>(this.baseUrl + 'user/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'user', member).pipe(
      tap(() => {
        this.members.update(members => members.map(m => m.username === member.username ? member : m))
      })
    )
  }

  setProfilePic(photo: Photo){
    return this.http.put(this.baseUrl + 'user/set-profile-picture/' + photo.id, {}).pipe(
      tap(()=> {
        this.members.update(members => members.map(m => {
          if(m.photos.includes(photo)){
            m.photoUrl = photo.url
          }
          return m;
        }))
      })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'user/delete-photo/' + photo.id).pipe(
      tap(()=> {
        this.members.update(members => members.map(m => {
          if(m.photos.includes(photo)){
            m.photos = m.photos.filter(x => x.id !== photo.id)
          }
          return m;
        }))
      })
    )
  }

}
