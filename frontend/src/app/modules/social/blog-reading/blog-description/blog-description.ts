import { Component, computed, input } from '@angular/core';
import { marked } from 'marked';

@Component({
  imports: [],
  selector: 'app-blog-description',
  styleUrl: './blog-description.scss',
  templateUrl: './blog-description.html',
})
export class BlogDescription {
  readonly markdown = input.required<string>();

  protected readonly html = computed(() => marked.parse(this.markdown(), { async: false }));
}
