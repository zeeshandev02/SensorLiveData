import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { SseService } from './services/sse.service';
import { StateService } from './services/state.service';
import { ApiService } from './services/api.service';
import { SensorReading } from './models/sensor-reading.model';
import { Alert } from './models/alert.model';
import { ReadingAggregate } from './models/reading-aggregate.model';
import { LiveChartComponent } from './components/live-chart/live-chart.component';
import { StatsPanelComponent } from './components/stats-panel/stats-panel.component';
import { AlertsPanelComponent } from './components/alerts-panel/alerts-panel.component';
import { TestControlsComponent } from './components/test-controls/test-controls.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, LiveChartComponent, StatsPanelComponent, AlertsPanelComponent, TestControlsComponent],
  template: `
    <div class="container">
      <header class="card">
        <h1>Real-time Analytics Dashboard</h1>
        <div class="status-indicator" [class.status-connected]="isConnected" [class.status-disconnected]="!isConnected"></div>
        <span>{{ isConnected ? 'Connected' : 'Disconnected' }}</span>
      </header>

      <app-test-controls></app-test-controls>

      <div class="grid grid-3">
        <div class="card">
          <h2>Live Chart</h2>
          <app-live-chart></app-live-chart>
        </div>

        <div class="card">
          <h2>Statistics</h2>
          <app-stats-panel></app-stats-panel>
        </div>

        <div class="card">
          <h2>Alerts</h2>
          <app-alerts-panel></app-alerts-panel>
        </div>
      </div>
    </div>
  `,
  styles: [`
    header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 20px;
    }
    
    h1 {
      margin: 0;
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  isConnected = false;
  private subscriptions: Subscription[] = [];

  constructor(
    private sseService: SseService,
    private stateService: StateService,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.initializeData();
    this.startStreaming();
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  private initializeData(): void {
    // Load initial data
    this.apiService.getLatestReadings(5000).subscribe(readings => {
      readings.forEach(reading => this.stateService.addReading(reading));
    });

    this.apiService.getRecentAlerts(100).subscribe(alerts => {
      alerts.forEach(alert => this.stateService.addAlert(alert));
    });

    this.apiService.getAggregates(60).subscribe(aggregates => {
      this.stateService.updateAggregates(aggregates);
    });
  }

  private startStreaming(): void {
    // Stream readings
    const readingsSub = this.sseService.getReadingsStream().subscribe({
      next: (reading: SensorReading) => {
        this.stateService.addReading(reading);
        this.stateService.updateConnectionStatus(true);
      },
      error: (error) => {
        console.error('Readings stream error:', error);
        this.stateService.updateConnectionStatus(false);
      }
    });

    // Stream alerts
    const alertsSub = this.sseService.getAlertsStream().subscribe({
      next: (alert: Alert) => {
        this.stateService.addAlert(alert);
      },
      error: (error) => {
        console.error('Alerts stream error:', error);
      }
    });

    this.subscriptions.push(readingsSub, alertsSub);
  }

  private startPolling(): void {
    // Poll aggregates every 5 seconds as fallback
    setInterval(() => {
      this.apiService.getAggregates(60).subscribe(aggregates => {
        this.stateService.updateAggregates(aggregates);
      });
    }, 5000);
  }
}
