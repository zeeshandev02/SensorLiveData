import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { StateService } from '../../services/state.service';
import { ApiService } from '../../services/api.service';
import { SensorReading } from '../../models/sensor-reading.model';
import { Alert } from '../../models/alert.model';

@Component({
  selector: 'app-test-controls',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="test-controls">
      <h3>Test Controls & Verification</h3>
      
      <div class="control-section">
        <h4>Data Injection</h4>
        <div class="control-group">
          <label>Sensor ID:</label>
          <select [(ngModel)]="selectedSensorId">
            <option value="TEMP_001">TEMP_001</option>
            <option value="TEMP_002">TEMP_002</option>
            <option value="PRESSURE_001">PRESSURE_001</option>
            <option value="HUMIDITY_001">HUMIDITY_001</option>
          </select>
        </div>
        
        <div class="control-group">
          <label>Value:</label>
          <input type="number" [(ngModel)]="testValue" step="0.01" placeholder="Enter test value">
        </div>
        
        <div class="control-group">
          <label>Unit:</label>
          <select [(ngModel)]="testUnit">
            <option value="°C">°C</option>
            <option value="hPa">hPa</option>
            <option value="%">%</option>
          </select>
        </div>
        
        <div class="control-group">
          <label>Location:</label>
          <input type="text" [(ngModel)]="testLocation" placeholder="Enter location">
        </div>
        
        <button class="btn" (click)="injectTestReading()" [disabled]="!testValue">
          Inject Test Reading
        </button>
        
        <button class="btn btn-secondary" (click)="injectAnomalyReading()">
          Inject Anomaly Reading
        </button>
      </div>
      
      <div class="control-section">
        <h4>Bulk Data Generation</h4>
        <div class="control-group">
          <label>Number of Readings:</label>
          <input type="number" [(ngModel)]="bulkCount" min="1" max="1000" value="100">
        </div>
        
        <div class="control-group">
          <label>Interval (ms):</label>
          <input type="number" [(ngModel)]="bulkInterval" min="10" max="5000" value="100">
        </div>
        
        <button class="btn" (click)="startBulkGeneration()" [disabled]="isGenerating">
          {{ isGenerating ? 'Generating...' : 'Start Bulk Generation' }}
        </button>
        
        <button class="btn btn-danger" (click)="stopBulkGeneration()" [disabled]="!isGenerating">
          Stop Generation
        </button>
      </div>
      
      <div class="control-section">
        <h4>System Verification</h4>
        <div class="verification-stats">
          <div class="stat-item">
            <span class="stat-label">Total Readings:</span>
            <span class="stat-value">{{ totalReadings }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Total Alerts:</span>
            <span class="stat-value">{{ totalAlerts }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Connection Status:</span>
            <span class="stat-value" [class.connected]="isConnected" [class.disconnected]="!isConnected">
              {{ isConnected ? 'Connected' : 'Disconnected' }}
            </span>
          </div>
          <div class="stat-item">
            <span class="stat-label">Last Update:</span>
            <span class="stat-value">{{ lastUpdate | date:'HH:mm:ss' }}</span>
          </div>
        </div>
        
        <button class="btn" (click)="refreshData()">
          Refresh Data
        </button>
        
        <button class="btn btn-warning" (click)="clearAllData()">
          Clear All Data
        </button>
      </div>
      
      <div class="control-section">
        <h4>Performance Test</h4>
        <div class="control-group">
          <label>Test Duration (seconds):</label>
          <input type="number" [(ngModel)]="testDuration" min="5" max="60" value="10">
        </div>
        
        <button class="btn" (click)="startPerformanceTest()" [disabled]="isPerformanceTesting">
          {{ isPerformanceTesting ? 'Testing...' : 'Start Performance Test' }}
        </button>
        
        <div *ngIf="performanceResults" class="performance-results">
          <h5>Performance Results:</h5>
          <p>Readings Generated: {{ performanceResults.readingsGenerated }}</p>
          <p>Alerts Generated: {{ performanceResults.alertsGenerated }}</p>
          <p>Average Response Time: {{ performanceResults.avgResponseTime }}ms</p>
          <p>Memory Usage: {{ performanceResults.memoryUsage }}MB</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .test-controls {
      background: #f8f9fa;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    
    .control-section {
      margin-bottom: 25px;
      padding: 15px;
      background: white;
      border-radius: 6px;
      border-left: 4px solid #007bff;
    }
    
    .control-section h4 {
      margin: 0 0 15px 0;
      color: #333;
    }
    
    .control-group {
      display: flex;
      align-items: center;
      margin-bottom: 10px;
      gap: 10px;
    }
    
    .control-group label {
      min-width: 120px;
      font-weight: bold;
      color: #555;
    }
    
    .control-group input,
    .control-group select {
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
      flex: 1;
    }
    
    .btn {
      background: #007bff;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      cursor: pointer;
      margin-right: 10px;
      margin-bottom: 10px;
    }
    
    .btn:hover:not(:disabled) {
      background: #0056b3;
    }
    
    .btn:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }
    
    .btn-secondary {
      background: #6c757d;
    }
    
    .btn-danger {
      background: #dc3545;
    }
    
    .btn-warning {
      background: #ffc107;
      color: #212529;
    }
    
    .verification-stats {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 15px;
      margin-bottom: 15px;
    }
    
    .stat-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px;
      background: #f8f9fa;
      border-radius: 4px;
    }
    
    .stat-label {
      font-weight: bold;
      color: #555;
    }
    
    .stat-value {
      font-weight: bold;
      color: #007bff;
    }
    
    .stat-value.connected {
      color: #28a745;
    }
    
    .stat-value.disconnected {
      color: #dc3545;
    }
    
    .performance-results {
      margin-top: 15px;
      padding: 15px;
      background: #e9ecef;
      border-radius: 4px;
    }
    
    .performance-results h5 {
      margin: 0 0 10px 0;
      color: #333;
    }
    
    .performance-results p {
      margin: 5px 0;
      color: #555;
    }
  `]
})
export class TestControlsComponent implements OnInit, OnDestroy {
  // Test data properties
  selectedSensorId = 'TEMP_001';
  testValue: number | null = null;
  testUnit = '°C';
  testLocation = 'Test Room';
  
  // Bulk generation properties
  bulkCount = 100;
  bulkInterval = 100;
  isGenerating = false;
  private bulkIntervalId?: number;
  
  // Verification properties
  totalReadings = 0;
  totalAlerts = 0;
  isConnected = false;
  lastUpdate = new Date();
  
  // Performance test properties
  testDuration = 10;
  isPerformanceTesting = false;
  performanceResults: any = null;
  
  private subscriptions: Subscription[] = [];

  constructor(
    private stateService: StateService,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    // Subscribe to state changes
    this.subscriptions.push(
      this.stateService.readings$.subscribe(readings => {
        this.totalReadings = readings.length;
        this.lastUpdate = new Date();
      })
    );

    this.subscriptions.push(
      this.stateService.alerts$.subscribe(alerts => {
        this.totalAlerts = alerts.length;
      })
    );

    this.subscriptions.push(
      this.stateService.connectionStatus$.subscribe(connected => {
        this.isConnected = connected;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
    this.stopBulkGeneration();
  }

  injectTestReading(): void {
    if (!this.testValue) return;

    const reading: SensorReading = {
      id: Date.now(),
      sensorId: this.selectedSensorId,
      value: this.testValue,
      timestamp: new Date().toISOString(),
      unit: this.testUnit,
      location: this.testLocation
    };

    this.stateService.addReading(reading);
    console.log('Test reading injected:', reading);
  }

  injectAnomalyReading(): void {
    // Generate an extreme value to trigger anomaly detection
    const extremeValue = Math.random() > 0.5 ? 100 : -50;
    
    const reading: SensorReading = {
      id: Date.now(),
      sensorId: this.selectedSensorId,
      value: extremeValue,
      timestamp: new Date().toISOString(),
      unit: this.testUnit,
      location: this.testLocation
    };

    this.stateService.addReading(reading);
    console.log('Anomaly reading injected:', reading);
  }

  startBulkGeneration(): void {
    if (this.isGenerating) return;

    this.isGenerating = true;
    let count = 0;

    this.bulkIntervalId = window.setInterval(() => {
      if (count >= this.bulkCount) {
        this.stopBulkGeneration();
        return;
      }

      const reading: SensorReading = {
        id: Date.now() + count,
        sensorId: this.selectedSensorId,
        value: 20 + Math.random() * 10,
        timestamp: new Date().toISOString(),
        unit: this.testUnit,
        location: this.testLocation
      };

      this.stateService.addReading(reading);
      count++;
    }, this.bulkInterval);
  }

  stopBulkGeneration(): void {
    if (this.bulkIntervalId) {
      clearInterval(this.bulkIntervalId);
      this.bulkIntervalId = undefined;
    }
    this.isGenerating = false;
  }

  refreshData(): void {
    // Refresh data from API
    this.apiService.getLatestReadings(1000).subscribe(readings => {
      readings.forEach(reading => this.stateService.addReading(reading));
    });

    this.apiService.getRecentAlerts(100).subscribe(alerts => {
      alerts.forEach(alert => this.stateService.addAlert(alert));
    });
  }

  clearAllData(): void {
    if (confirm('Are you sure you want to clear all data?')) {
      // Clear local state
      this.stateService['readingsSubject'].next([]);
      this.stateService['alertsSubject'].next([]);
      console.log('All data cleared');
    }
  }

  startPerformanceTest(): void {
    if (this.isPerformanceTesting) return;

    this.isPerformanceTesting = true;
    this.performanceResults = null;
    
    const startTime = performance.now();
    const startMemory = (performance as any).memory?.usedJSHeapSize || 0;
    let readingsGenerated = 0;
    let alertsGenerated = 0;

    const testInterval = setInterval(() => {
      const reading: SensorReading = {
        id: Date.now() + readingsGenerated,
        sensorId: this.selectedSensorId,
        value: 20 + Math.random() * 10,
        timestamp: new Date().toISOString(),
        unit: this.testUnit,
        location: this.testLocation
      };

      this.stateService.addReading(reading);
      readingsGenerated++;

      // Simulate alert generation
      if (Math.random() < 0.1) {
        const alert: Alert = {
          id: Date.now() + alertsGenerated,
          sensorId: this.selectedSensorId,
          type: 'Test Alert',
          message: 'Performance test alert',
          value: reading.value,
          threshold: 25,
          timestamp: new Date().toISOString(),
          isResolved: false
        };
        this.stateService.addAlert(alert);
        alertsGenerated++;
      }
    }, 10);

    setTimeout(() => {
      clearInterval(testInterval);
      this.isPerformanceTesting = false;
      
      const endTime = performance.now();
      const endMemory = (performance as any).memory?.usedJSHeapSize || 0;
      
      this.performanceResults = {
        readingsGenerated,
        alertsGenerated,
        avgResponseTime: Math.round((endTime - startTime) / readingsGenerated),
        memoryUsage: Math.round((endMemory - startMemory) / 1024 / 1024)
      };
    }, this.testDuration * 1000);
  }
}
