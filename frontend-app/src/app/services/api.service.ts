import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SensorReading } from '../models/sensor-reading.model';
import { Alert } from '../models/alert.model';
import { ReadingAggregate } from '../models/reading-aggregate.model';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) { }

  getLatestReadings(limit: number = 1000): Observable<SensorReading[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<SensorReading[]>(`${this.baseUrl}/readings/latest`, { params });
  }

  getAggregates(windowSeconds: number = 60): Observable<ReadingAggregate[]> {
    const params = new HttpParams().set('windowSeconds', windowSeconds.toString());
    return this.http.get<ReadingAggregate[]>(`${this.baseUrl}/readings/aggregates`, { params });
  }

  getRecentAlerts(limit: number = 100): Observable<Alert[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<Alert[]>(`${this.baseUrl}/alerts`, { params });
  }
}
