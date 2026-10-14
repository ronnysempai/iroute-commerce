import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ProcessResult, QuarantineRecord, UploadResult } from '../models/commerce.models';

/** Consume los endpoints del comercio expuestos por la API. */
@Injectable({ providedIn: 'root' })
export class CommerceService {
  private readonly apiUrl = `${environment.apiUrl}/api/commerce`;

  constructor(private readonly http: HttpClient) {}

  /** Carga un archivo commerce_DDMMYYYY.csv. */
  upload(file: File): Observable<UploadResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<UploadResult>(`${this.apiUrl}/upload`, formData);
  }

  /** Procesa los registros de una fecha (mueve invalidos a cuarentena). */
  process(pcProcessdate: string): Observable<ProcessResult> {
    return this.http.post<ProcessResult>(`${this.apiUrl}/process`, { pcProcessdate });
  }

  /** Lista los registros en cuarentena. */
  getQuarantine(): Observable<QuarantineRecord[]> {
    return this.http.get<QuarantineRecord[]>(`${this.apiUrl}/quarantine`);
  }
}
