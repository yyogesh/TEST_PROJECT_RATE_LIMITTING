import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KenticoService, Article } from '../../services/kentico.service';

@Component({
  selector: 'app-article',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './article.html',
  styleUrls: ['./article.scss']
})
export class ArticleComponent implements OnInit {
  // Using Angular 21 signals
  articles = signal<Article[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // Computed signal for derived state
  hasArticles = computed(() => this.articles().length > 0);
  isLoading = computed(() => this.loading());

  constructor(private kenticoService: KenticoService) { }

  ngOnInit(): void {
    this.loadArticles();
  }

  loadArticles(): void {
    this.loading.set(true);
    this.error.set(null);

    this.kenticoService.getArticles().subscribe({
      next: (data) => {
        this.articles.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(error.message || 'Failed to load articles');
        this.loading.set(false);
        console.error('Error loading articles:', error);
      }
    });
  }

  refreshArticles(): void {
    this.loadArticles();
  }
}