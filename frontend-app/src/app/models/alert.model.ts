export interface Alert {
  id: number;
  sensorId: string;
  type: string;
  message: string;
  value: number;
  threshold: number;
  timestamp: string;
  isResolved: boolean;
}
