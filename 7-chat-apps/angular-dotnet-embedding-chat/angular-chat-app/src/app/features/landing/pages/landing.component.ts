import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Step {
  number: string;
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, ButtonModule, CardModule],
  template: `
    <div class="min-h-screen flex flex-col" style="background: #ffffff; color: #111827">

      <!-- Navbar -->
      <header class="sticky top-0 z-50 flex items-center justify-between px-8 h-16 shadow-sm"
              style="background: #ffffff; border-bottom: 1px solid #e5e7eb">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-lg flex items-center justify-center"
               style="background: var(--p-primary-color)">
            <i class="pi pi-comments text-sm" style="color: white"></i>
          </div>
          <span class="font-bold text-lg" style="color: #111827">DocChat</span>
        </div>
        <div class="flex items-center gap-3">
          <a routerLink="/login">
            <p-button label="Sign In" severity="secondary" variant="outlined" size="small" />
          </a>
          <a routerLink="/login" [queryParams]="{ mode: 'register' }">
            <p-button label="Get Started Free" size="small" icon="pi pi-arrow-right" iconPos="right" />
          </a>
        </div>
      </header>

      <!-- Hero -->
      <section class="flex flex-col items-center justify-center text-center px-6 py-24 relative overflow-hidden"
               style="background: linear-gradient(135deg, #f0f4ff 0%, #ffffff 60%, #f0f9f0 100%)">
        <div class="absolute top-0 left-0 w-full h-full pointer-events-none"
             style="background: radial-gradient(ellipse 60% 50% at 50% 0%, rgba(99,102,241,0.08) 0%, transparent 70%)"></div>

        <div class="inline-flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium mb-6"
             style="background: var(--p-primary-50); color: var(--p-primary-color); border: 1px solid var(--p-primary-100)">
          <i class="pi pi-sparkles text-xs"></i>
          <span>Powered by Azure OpenAI &amp; RAG</span>
        </div>

        <h1 class="text-5xl font-extrabold leading-tight m-0 max-w-3xl" style="color: #111827">
          Chat with Your Documents,<br>
          <span style="color: var(--p-primary-color)">Instantly.</span>
        </h1>
        <p class="text-xl mt-6 mb-8 max-w-xl" style="color: #6b7280">
          Upload any PDF or Word document, generate AI embeddings, and ask questions in natural language — all in minutes.
        </p>

        <div class="flex items-center gap-4 flex-wrap justify-center">
          <a routerLink="/login" [queryParams]="{ mode: 'register' }">
            <p-button label="Get Started Free" icon="pi pi-arrow-right" iconPos="right" size="large" />
          </a>
          <a routerLink="/login">
            <p-button label="Sign In" severity="secondary" variant="outlined" size="large" />
          </a>
        </div>

        <p class="text-sm mt-6 m-0" style="color: #9ca3af">No credit card required &middot; Free to start</p>
      </section>

      <!-- Features -->
      <section class="px-6 py-20" style="background: #f9fafb">
        <div class="max-w-5xl mx-auto">
          <div class="text-center mb-12">
            <h2 class="text-3xl font-bold m-0" style="color: #111827">Everything you need</h2>
            <p class="text-base mt-3 m-0" style="color: #6b7280">From upload to insight in three simple steps</p>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            @for (f of features; track f.title) {
              <div class="rounded-2xl p-6 flex flex-col gap-4 shadow-sm"
                   style="background: #ffffff; border: 1px solid #e5e7eb">
                <div class="w-12 h-12 rounded-xl flex items-center justify-center"
                     style="background: var(--p-primary-50)">
                  <i [class]="'pi ' + f.icon + ' text-xl'" style="color: var(--p-primary-color)"></i>
                </div>
                <div>
                  <h3 class="font-semibold text-base m-0" style="color: #111827">{{ f.title }}</h3>
                  <p class="text-sm mt-1 m-0" style="color: #6b7280">{{ f.description }}</p>
                </div>
              </div>
            }
          </div>
        </div>
      </section>

      <!-- How it works -->
      <section class="px-6 py-20" style="background: #ffffff">
        <div class="max-w-4xl mx-auto">
          <div class="text-center mb-12">
            <h2 class="text-3xl font-bold m-0" style="color: #111827">How it works</h2>
            <p class="text-base mt-3 m-0" style="color: #6b7280">Up and running in under 2 minutes</p>
          </div>
          <div class="flex flex-col md:flex-row gap-8 items-start">
            @for (step of steps; track step.number) {
              <div class="flex-1 flex flex-col items-center text-center gap-4">
                <div class="w-14 h-14 rounded-2xl flex items-center justify-center shadow-sm relative"
                     style="background: var(--p-primary-color)">
                  <i [class]="'pi ' + step.icon + ' text-xl'" style="color: white"></i>
                  <span class="absolute -top-2 -right-2 w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold"
                        style="background: #111827; color: white">{{ step.number }}</span>
                </div>
                <div>
                  <h3 class="font-semibold m-0" style="color: #111827">{{ step.title }}</h3>
                  <p class="text-sm mt-1 m-0" style="color: #6b7280">{{ step.description }}</p>
                </div>
              </div>

              @if (!$last) {
                <div class="hidden md:flex items-center pt-7" style="color: #d1d5db">
                  <i class="pi pi-arrow-right text-xl"></i>
                </div>
              }
            }
          </div>
        </div>
      </section>

      <!-- CTA Banner -->
      <section class="px-6 py-20" style="background: var(--p-primary-color)">
        <div class="max-w-2xl mx-auto text-center">
          <h2 class="text-3xl font-bold m-0" style="color: white">Ready to get started?</h2>
          <p class="text-base mt-3 mb-8 m-0" style="color: rgba(255,255,255,0.8)">
            Join and start chatting with your documents today.
          </p>
          <a routerLink="/login" [queryParams]="{ mode: 'register' }">
            <p-button label="Create Free Account" icon="pi pi-user-plus"
              severity="contrast" size="large" />
          </a>
        </div>
      </section>

      <!-- Footer -->
      <footer class="px-8 py-6 flex items-center justify-between"
              style="background: #111827; color: #9ca3af">
        <div class="flex items-center gap-2">
          <i class="pi pi-comments" style="color: var(--p-primary-color)"></i>
          <span class="font-semibold text-sm" style="color: #ffffff">DocChat</span>
        </div>
        <p class="text-xs m-0">&copy; 2025 DocChat. All rights reserved.</p>
      </footer>
    </div>
  `
})
export class LandingComponent {
  features: Feature[] = [
    {
      icon: 'pi-cloud-upload',
      title: 'Upload Any Document',
      description: 'Supports PDF, DOC, and DOCX files up to 20 MB. Drag, drop, and you\'re done.'
    },
    {
      icon: 'pi-microchip-ai',
      title: 'AI Embeddings Instantly',
      description: 'Documents are chunked and embedded using state-of-the-art vector models automatically.'
    },
    {
      icon: 'pi-comments',
      title: 'Chat in Natural Language',
      description: 'Ask any question. The AI finds the most relevant passages and answers with context.'
    }
  ];

  steps: Step[] = [
    {
      number: '1',
      icon: 'pi-upload',
      title: 'Upload',
      description: 'Drop your PDF or Word document into the upload zone.'
    },
    {
      number: '2',
      icon: 'pi-sparkles',
      title: 'Embed',
      description: 'We generate semantic embeddings so your document is searchable by meaning.'
    },
    {
      number: '3',
      icon: 'pi-comments',
      title: 'Chat',
      description: 'Ask questions and get precise, sourced answers from your document.'
    }
  ];
}
