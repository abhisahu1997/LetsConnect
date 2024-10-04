import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../models/member';
import { of, tap } from 'rxjs';
import { Photo } from '../models/photo';
import { PaginatedResult } from '../models/pagination';
import { UserParams } from '../models/userParams';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeader } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {

  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  user = this.accountService.currentUser();
  memberCache = new Map();
  userParams = signal<UserParams>(new UserParams(this.user));

  constructor() { }

  resetUserParams(){
    this.userParams.set(new UserParams(this.user));
  }

  getMembers(){
    const response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if(response) return setPaginatedResponse(response, this.paginatedResult)
    let params = setPaginationHeader(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);
    return this.http.get<Member[]>(this.baseUrl + 'user', {observe: 'response', params}).subscribe({
      next: response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response)
      }
    });
  }

  getMember(username: string){

    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elm) => arr.concat(elm.body), [])
      .find((m: Member) => m.username === username);
    
    if(member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'user/' + username);
  }

  updateMember(member: Member){
    return this.http.put(this.baseUrl + 'user', member).pipe(
      // tap(() => {
      //   this.members.update(members => members.map(m => m.username === member.username ? member : m))
      // })
    )
  }

  setProfilePic(photo: Photo){
    return this.http.put(this.baseUrl + 'user/set-profile-picture/' + photo.id, {}).pipe(
      // tap(()=> {
      //   this.members.update(members => members.map(m => {
      //     if(m.photos.includes(photo)){
      //       m.photoUrl = photo.url
      //     }
      //     return m;
      //   }))
      // })
    )
  }

  deletePhoto(photo: Photo){
    return this.http.delete(this.baseUrl + 'user/delete-photo/' + photo.id).pipe(
      // tap(()=> {
      //   this.members.update(members => members.map(m => {
      //     if(m.photos.includes(photo)){
      //       m.photos = m.photos.filter(x => x.id !== photo.id)
      //     }
      //     return m;
      //   }))
      // })
    )
  }

}
