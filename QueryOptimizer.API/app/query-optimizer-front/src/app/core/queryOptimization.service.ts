import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AppUser } from "./auth.service";
import { QueryOptimizationRequest } from "../models/queryOptimizationRequest";
import { QueryOptimizationResult } from "../models/queryOptimizationResult";

@Injectable({
  providedIn: 'root'
})
export class QueryOptimizationService {
  private readonly baseUrl = 'https://localhost:7149/api';

  constructor(private readonly http: HttpClient) {}

  analyze(request: QueryOptimizationRequest): Observable<QueryOptimizationResult> {
    return this.http.post<QueryOptimizationResult>(
      `${this.baseUrl}/QueryOptimizer/Analyze`,
      request
    );
  }
}