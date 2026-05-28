import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { QueryAnalysisRun } from "../models/queryAnalysisRun";

@Injectable({
    providedIn: 'root'
})
export class HistoryService{
    private readonly baseUrl = 'https://localhost:7149/api';

    constructor(private readonly http: HttpClient){

    }

    getUserHistory(userId: number): Observable<QueryAnalysisRun[]> {
        return this.http.get<QueryAnalysisRun[]>(
          `${this.baseUrl}/History/UserHistory/${userId}`
        );
    }

    getAnalysisDetails(analysisRunId: number): Observable<any> {
      return this.http.get<any>(
        `${this.baseUrl}/History/Analysis/${analysisRunId}`
      );
    }
}