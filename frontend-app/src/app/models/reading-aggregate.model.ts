export interface ReadingAggregate {
  sensorId: string;
  min: number;
  max: number;
  average: number;
  count: number;
  windowStart: string;
  windowEnd: string;
  throughput: number;
}
