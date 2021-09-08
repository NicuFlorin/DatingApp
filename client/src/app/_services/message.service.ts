import { environment } from './../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { getPaginatedResut, getPaginationHeaders } from './paginationHelper';
import { Message } from '../models/message';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) {}

  getMesssages(pageNumber, pageSize, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);

    params = params.append('Container', container);

    return getPaginatedResut<Message[]>(
      this.baseUrl + 'messages',
      params,
      this.http
    );
  }

  getMessageThread(userName: string) {
    return this.http.get<Message[]>(
      this.baseUrl + 'messages/thread/' + userName
    );
  }

  sendMessage(userName: string, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages', {
      recipientUsername: userName,
      content,
    });
  }

  deleteMessage(Id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + Id);
  }
}
