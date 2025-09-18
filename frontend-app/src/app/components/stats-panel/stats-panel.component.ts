import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { StateService } from '../../services/state.service';
import { ReadingAggregate } from '../../models/reading-aggregate.model';
import { SensorReading } from '../../models/sensor-reading.model';

@Component({
  selector: 'app-stats-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stats-grid">
      <div class="stat-card" *ngFor="let stat of currentStats">
        <div class="stat-value">{{ stat.value | number:'1.2-2' }}</div>
        <div class="stat-label">{{ stat.label }}</div>
      </div>
    </div>
    
    <div class="sensor-stats" *ngIf="sensorAggregates.length > 0">
      <h3>Sensor Details</h3>
      <div class="sensor-list">
        <div class="sensor-item" *ngFor="let sensor of sensorAggregates">
          <div class="sensor-id">{{ sensor.sensorId }}</div>
          <div class="sensor-values">
            <span>Min: {{ sensor.min | number:'1.2-2' }}</span>
            <span>Max: {{ sensor.max | number:'1.2-2' }}</span>
            <span>Avg: {{ sensor.average | number:'1.2-2' }}</span>
            <span>Count: {{ sensor.count }}</span>
            <span>Throughput: {{ sensor.throughput | number:'1.2-2' }}/s</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .sensor-stats {
      margin-top: 20px;
    }
    
    .sensor-stats h3 {
      margin-bottom: 10px;
      color: #333;
    }
    
    .sensor-list {
      max-height: 200px;
      overflow-y: auto;
    }
    
    .sensor-item {
      background: #f8f9fa;
      padding: 10px;
      margin-bottom: 8px;
      border-radius: 4px;
      border-left: 3px solid #007bff;
    }
    
    .sensor-id {
      font-weight: bold;
      margin-bottom: 5px;
      color: #007bff;
    }
    
    .sensor-values {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
      gap: 5px;
      font-size: 12px;
      color: #666;
    }
  `]
})
export class StatsPanelComponent implements OnInit, OnDestroy {
  currentStats: Array<{ label: string; value: number }> = [];
  sensorAggregates: ReadingAggregate[] = [];
  private subscriptions: Subscription[] = [];

  constructor(private stateService: StateService) {}

  ngOnInit(): void {
    // Subscribe to aggregates updates
    const aggregatesSub = this.stateService.aggregates$.subscribe(aggregates => {
      this.sensorAggregates = aggregates;
      this.updateCurrentStats(aggregates);
    });

    // Subscribe to readings for real-time stats
    const readingsSub = this.stateService.readings$.subscribe(readings => {
      this.updateRealTimeStats(readings);
    });

    this.subscriptions.push(aggregatesSub, readingsSub);
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private updateCurrentStats(aggregates: ReadingAggregate[]): void {
    if (aggregates.length === 0) return;

    const totalReadings = aggregates.reduce((sum, agg) => sum + agg.count, 0);
    const totalThroughput = aggregates.reduce((sum, agg) => sum + agg.throughput, 0);
    const avgMin = aggregates.reduce((sum, agg) => sum + agg.min, 0) / aggregates.length;
    const avgMax = aggregates.reduce((sum, agg) => sum + agg.max, 0) / aggregates.length;
    const avgAverage = aggregates.reduce((sum, agg) => sum + agg.average, 0) / aggregates.length;

    this.currentStats = [
      { label: 'Total Readings', value: totalReadings },
      { label: 'Total Throughput/s', value: totalThroughput },
      { label: 'Avg Min Value', value: avgMin },
      { label: 'Avg Max Value', value: avgMax },
      { label: 'Avg Value', value: avgAverage },
      { label: 'Active Sensors', value: aggregates.length }
    ];
  }

  private updateRealTimeStats(readings: SensorReading[]): void {
    if (readings.length === 0) return;

    const recentReadings = readings.slice(-1000); // Last 1000 readings
    const min = Math.min(...recentReadings.map(r => r.value));
    const max = Math.max(...recentReadings.map(r => r.value));
    const avg = recentReadings.reduce((sum, r) => sum + r.value, 0) / recentReadings.length;

    // Update real-time stats if no aggregates available
    if (this.sensorAggregates.length === 0) {
      this.currentStats = [
        { label: 'Total Readings', value: readings.length },
        { label: 'Min Value', value: min },
        { label: 'Max Value', value: max },
        { label: 'Avg Value', value: avg },
        { label: 'Recent Count', value: recentReadings.length }
      ];
    }
  }
}
