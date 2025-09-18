import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { StateService } from '../../services/state.service';
import { Alert } from '../../models/alert.model';

@Component({
  selector: 'app-alerts-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="alerts-container">
      <div class="alerts-header">
        <h3>Recent Alerts ({{ alerts.length }})</h3>
        <button class="btn" (click)="clearAlerts()" [disabled]="alerts.length === 0">
          Clear All
        </button>
      </div>
      
      <div class="alerts-list" *ngIf="alerts.length > 0; else noAlerts">
        <div class="alert-item" 
             *ngFor="let alert of alerts; trackBy: trackByAlertId"
             [class.alert-warning]="alert.type === 'Anomaly'"
             [class.alert-danger]="alert.type === 'Critical'">
          <div class="alert-header">
            <span class="alert-type">{{ alert.type }}</span>
            <span class="alert-time">{{ formatTime(alert.timestamp) }}</span>
          </div>
          <div class="alert-sensor">{{ alert.sensorId }}</div>
          <div class="alert-message">{{ alert.message }}</div>
          <div class="alert-details">
            <span>Value: {{ alert.value | number:'1.2-2' }}</span>
            <span>Threshold: {{ alert.threshold | number:'1.2-2' }}</span>
          </div>
        </div>
      </div>
      
      <ng-template #noAlerts>
        <div class="no-alerts">
          <p>No alerts at this time</p>
        </div>
      </ng-template>
    </div>
  `,
  styles: [`
    .alerts-container {
      height: 400px;
      display: flex;
      flex-direction: column;
    }
    
    .alerts-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 15px;
      padding-bottom: 10px;
      border-bottom: 1px solid #eee;
    }
    
    .alerts-header h3 {
      margin: 0;
      color: #333;
    }
    
    .alerts-list {
      flex: 1;
      overflow-y: auto;
      max-height: 300px;
    }
    
    .alert-item {
      padding: 12px;
      margin-bottom: 8px;
      border-radius: 6px;
      border-left: 4px solid;
      background: #f8f9fa;
    }
    
    .alert-item.alert-warning {
      border-left-color: #ffc107;
      background: #fff3cd;
    }
    
    .alert-item.alert-danger {
      border-left-color: #dc3545;
      background: #f8d7da;
    }
    
    .alert-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 5px;
    }
    
    .alert-type {
      font-weight: bold;
      font-size: 14px;
    }
    
    .alert-time {
      font-size: 12px;
      color: #666;
    }
    
    .alert-sensor {
      font-weight: bold;
      color: #007bff;
      margin-bottom: 5px;
    }
    
    .alert-message {
      margin-bottom: 8px;
      font-size: 14px;
    }
    
    .alert-details {
      display: flex;
      gap: 15px;
      font-size: 12px;
      color: #666;
    }
    
    .no-alerts {
      text-align: center;
      padding: 40px 20px;
      color: #666;
    }
    
    .no-alerts p {
      margin: 0;
      font-style: italic;
    }
  `]
})
export class AlertsPanelComponent implements OnInit, OnDestroy {
  alerts: Alert[] = [];
  private subscription?: Subscription;

  constructor(private stateService: StateService) {}

  ngOnInit(): void {
    this.subscription = this.stateService.alerts$.subscribe(alerts => {
      this.alerts = alerts;
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  trackByAlertId(index: number, alert: Alert): number {
    return alert.id;
  }

  formatTime(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);
    const diffHours = Math.floor(diffMinutes / 60);

    if (diffSeconds < 60) {
      return `${diffSeconds}s ago`;
    } else if (diffMinutes < 60) {
      return `${diffMinutes}m ago`;
    } else if (diffHours < 24) {
      return `${diffHours}h ago`;
    } else {
      return date.toLocaleDateString();
    }
  }

  clearAlerts(): void {
    // In a real implementation, you might want to call an API to clear alerts
    // For now, we'll just clear the local state
    this.stateService['alertsSubject'].next([]);
  }
}
