import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Member } from '../models/member';
import { PaginatedResult } from '../models/pagination';
import { setPaginatedResponse, setPaginationHeader } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class LikesService {
  baseUrl = environment.apiUrl;
  private http = inject(HttpClient);
  likeIds = signal<number[]>([]);
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);

  toogleLike(targetId: number){
    return this.http.post(`${this.baseUrl}likes/${targetId}`, {})
  }

  getLikes(predicate: string, pageNymber: number, pageSize: number){
    let params = setPaginationHeader(pageNymber, pageSize);
    params = params.append('predicate', predicate);
    return this.http.get<Member[]>(`${this.baseUrl}likes`, {observe: 'response', params}).subscribe({
      next: response => setPaginatedResponse(response, this.paginatedResult)
    })
  }

  getLikeIds(){
    return this.http.get<number[]>(`${this.baseUrl}likes/list`).subscribe({
      next: ids => this.likeIds.set(ids)
    })
  }

  constructor() { }
}
