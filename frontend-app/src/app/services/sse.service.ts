import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SensorReading } from '../models/sensor-reading.model';
import { Alert } from '../models/alert.model';

@Injectable({
  providedIn: 'root'
})
export class SseService {
  private readonly baseUrl = 'http://localhost:5000/api/stream';

  getReadingsStream(): Observable<SensorReading> {
    return new Observable<SensorReading>(observer => {
      const eventSource = new EventSource(`${this.baseUrl}/readings`);
      
      eventSource.addEventListener('reading', (event) => {
        try {
          const data = JSON.parse(event.data) as SensorReading;
          observer.next(data);
        } catch (error) {
          console.error('Error parsing reading data:', error);
        }
      });

      eventSource.onerror = (error) => {
        console.error('SSE error:', error);
        observer.error(error);
      };

      return () => {
        eventSource.close();
      };
    });
  }

  getAlertsStream(): Observable<Alert> {
    return new Observable<Alert>(observer => {
      const eventSource = new EventSource(`${this.baseUrl}/alerts`);
      
      eventSource.addEventListener('alert', (event) => {
        try {
          const data = JSON.parse(event.data) as Alert;
          observer.next(data);
        } catch (error) {
          console.error('Error parsing alert data:', error);
        }
      });

      eventSource.onerror = (error) => {
        console.error('SSE error:', error);
        observer.error(error);
      };

      return () => {
        eventSource.close();
      };
    });
  }
}
