import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map, retry } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Article {
  title: string;
  body: string;
  id?: string;
  name?: string;
}

export interface GraphQLResponse {
  data: {
    articleArticle: Article | { nodes: Article[] };
  };
  errors?: Array<{ message: string }>;
}

@Injectable({
  providedIn: 'root'
})
export class KenticoService {
  private readonly http = inject(HttpClient); // Angular 21 inject() function
  private readonly apiUrl = environment.kenticoApiUrl;
  private readonly apiKey = environment.kenticoApiKey;

  /**
   * Get all articles from Kentico
   */
  getArticles(): Observable<Article[]> {
    const headers = this.getHeaders();
    
    const query = {
      query: `{
        articleArticle {
          title
          body
        }
      }`
    };

    return this.http.post<GraphQLResponse>(this.apiUrl, query, { headers })
  .pipe(
    retry(2), // Retry failed requests up to 2 times
    map((response): Article[] => {
      if (response.errors) {
        throw new Error(response.errors[0].message);
      }
      
      const articleData = response.data?.articleArticle;
      
      if (!articleData) {
        return [];
      }
      
      // Check if it's an array
      if (Array.isArray(articleData)) {
        return articleData;
      }
      
      // Check if it has nodes property (collection structure)
      if ('nodes' in articleData && Array.isArray(articleData.nodes)) {
        return articleData.nodes;
      }
      
      // Single item - wrap in array
      if ('title' in articleData && 'body' in articleData) {
        return [articleData as Article];
      }
      
      return [];
    }),
    catchError(this.handleError)
  );
  }

  /**
   * Get single article by title (example filter)
   */
  getArticleByTitle(title: string): Observable<Article> {
    const headers = this.getHeaders();
    
    const query = {
      query: `{
        articleArticle(where: { title: { eq: "${title}" } }) {
          title
          body
        }
      }`
    };

    return this.http.post<GraphQLResponse>(this.apiUrl, query, { headers })
      .pipe(
        map(response => {
          if (response.errors) {
            throw new Error(response.errors[0].message);
          }
          return response.data?.articleArticle as Article;
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Create HTTP headers with authorization
   */
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.apiKey}`
    });
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
      if (error.error?.errors) {
        errorMessage = error.error.errors[0].message;
      }
    }
    
    console.error('Kentico API Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}