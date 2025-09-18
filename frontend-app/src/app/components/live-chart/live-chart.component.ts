import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { StateService } from '../../services/state.service';
import { SensorReading } from '../../models/sensor-reading.model';

@Component({
  selector: 'app-live-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="chart-container">
      <canvas #chartCanvas width="400" height="200"></canvas>
    </div>
    <div class="chart-info">
      <p>Total readings: {{ totalReadings }}</p>
      <p>Displayed points: {{ displayedPoints }}</p>
    </div>
  `,
  styles: [`
    .chart-container {
      position: relative;
      height: 300px;
      margin-bottom: 10px;
    }
    
    .chart-info {
      font-size: 12px;
      color: #666;
    }
  `]
})
export class LiveChartComponent implements OnInit, OnDestroy {
  totalReadings = 0;
  displayedPoints = 0;
  private chart: any;
  private subscription?: Subscription;

  constructor(private stateService: StateService) {}

  ngOnInit(): void {
    this.initializeChart();
    this.subscription = this.stateService.readings$.subscribe(readings => {
      this.updateChart(readings);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private initializeChart(): void {
    // This is a placeholder for Chart.js initialization
    // In a real implementation, you would initialize Chart.js here
    console.log('Chart initialized');
  }

  private updateChart(readings: SensorReading[]): void {
    this.totalReadings = readings.length;
    
    // Decimate data for performance
    const decimatedReadings = this.stateService.decimateData(readings, 1000);
    this.displayedPoints = decimatedReadings.length;
    
    // Update chart with decimated data
    this.renderChart(decimatedReadings);
  }

  private renderChart(readings: SensorReading[]): void {
    // This is a simplified chart rendering
    // In a real implementation, you would use Chart.js or ng2-charts
    const canvas = document.querySelector('canvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (readings.length === 0) return;

    // Simple line chart rendering
    ctx.strokeStyle = '#007bff';
    ctx.lineWidth = 2;
    ctx.beginPath();

    const maxValue = Math.max(...readings.map(r => r.value));
    const minValue = Math.min(...readings.map(r => r.value));
    const range = maxValue - minValue || 1;

    readings.forEach((reading, index) => {
      const x = (index / (readings.length - 1)) * canvas.width;
      const y = canvas.height - ((reading.value - minValue) / range) * canvas.height;

      if (index === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    });

    ctx.stroke();

    // Add data points
    ctx.fillStyle = '#007bff';
    readings.forEach((reading, index) => {
      const x = (index / (readings.length - 1)) * canvas.width;
      const y = canvas.height - ((reading.value - minValue) / range) * canvas.height;
      
      ctx.beginPath();
      ctx.arc(x, y, 2, 0, 2 * Math.PI);
      ctx.fill();
    });
  }
}
