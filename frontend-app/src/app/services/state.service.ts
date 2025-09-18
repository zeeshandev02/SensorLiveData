import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { SensorReading } from '../models/sensor-reading.model';
import { Alert } from '../models/alert.model';
import { ReadingAggregate } from '../models/reading-aggregate.model';

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private readonly maxBufferSize = 100000; // 100k max readings
  private readonly maxAlertsSize = 10000; // 10k max alerts

  private readingsSubject = new BehaviorSubject<SensorReading[]>([]);
  private alertsSubject = new BehaviorSubject<Alert[]>([]);
  private aggregatesSubject = new BehaviorSubject<ReadingAggregate[]>([]);
  private connectionStatusSubject = new BehaviorSubject<boolean>(false);

  public readings$ = this.readingsSubject.asObservable();
  public alerts$ = this.alertsSubject.asObservable();
  public aggregates$ = this.aggregatesSubject.asObservable();
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  addReading(reading: SensorReading): void {
    const currentReadings = this.readingsSubject.value;
    const newReadings = [...currentReadings, reading];
    
    // Keep only the latest readings if we exceed the buffer size
    if (newReadings.length > this.maxBufferSize) {
      newReadings.splice(0, newReadings.length - this.maxBufferSize);
    }
    
    this.readingsSubject.next(newReadings);
  }

  addAlert(alert: Alert): void {
    const currentAlerts = this.alertsSubject.value;
    const newAlerts = [alert, ...currentAlerts];
    
    // Keep only the latest alerts if we exceed the buffer size
    if (newAlerts.length > this.maxAlertsSize) {
      newAlerts.splice(this.maxAlertsSize);
    }
    
    this.alertsSubject.next(newAlerts);
  }

  updateAggregates(aggregates: ReadingAggregate[]): void {
    this.aggregatesSubject.next(aggregates);
  }

  updateConnectionStatus(connected: boolean): void {
    this.connectionStatusSubject.next(connected);
  }

  getLatestReadings(count: number = 1000): SensorReading[] {
    const readings = this.readingsSubject.value;
    return readings.slice(-count);
  }

  getRecentAlerts(count: number = 100): Alert[] {
    const alerts = this.alertsSubject.value;
    return alerts.slice(0, count);
  }

  // Decimation function for chart performance
  decimateData(data: SensorReading[], maxPoints: number = 1000): SensorReading[] {
    if (data.length <= maxPoints) {
      return data;
    }

    // Simple decimation - take every nth point
    const step = Math.ceil(data.length / maxPoints);
    const decimated: SensorReading[] = [];
    
    for (let i = 0; i < data.length; i += step) {
      decimated.push(data[i]);
    }
    
    return decimated;
  }
}
