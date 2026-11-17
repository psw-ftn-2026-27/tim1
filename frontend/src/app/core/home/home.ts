import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../auth/auth';

interface HomeCapability {
  id: string;
  title: string;
  description: string;
  action: string;
  guestAction?: string;
  status: 'available' | 'unimplemented' | 'unconfirmed';
  route: string | null;
  message: string;
  image: string;
  imageAlt: string;
  requiresLogin: boolean;
}

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly auth = inject(Auth);
  // Availability is maintained manually after agreement with the owning team.
  protected readonly capabilities: readonly HomeCapability[] = [
    {
      id: 'tours',
      title: 'Ture i priče',
      description: 'Istraži mesta kroz autentične priče i objavljene ture.',
      action: 'Istraži ture',
      guestAction: 'Prijavi se za ture',
      status: 'available',
      route: '/exploration',
      message: 'Za pregled objavljenih tura potrebno je da se prijaviš.',
      image: 'modern-tours.webp',
      imageAlt: 'Putnik sa rancem u živopisnoj ulici istorijskog grada',
      requiresLogin: true,
    },
    {
      id: 'games',
      title: 'Igre i izazovi',
      description:
        'Otkrij kako igre i interaktivne mape proširuju iskustvo istraživanja. Nove mogućnosti su u pripremi.',
      action: 'Istraži igre',
      guestAction: 'Prijavi se za igre',
      status: 'available',
      route: '/games',
      message: 'Igre i izazovi uskoro će biti dostupni.',
      image: 'modern-games.webp',
      imageAlt: 'Troje mladih istraživača čita papirnu mapu ispred planina',
      requiresLogin: true,
    },
    {
      id: 'community',
      title: 'Zajednica',
      description: 'Pronađi inspiraciju u blogovima drugih istraživača.',
      action: 'Pogledaj blogove',
      guestAction: 'Prijavi se za blogove',
      status: 'available',
      route: '/social',
      message: 'Za pregled objavljenih blogova potrebno je da se prijaviš.',
      image: 'modern-community.webp',
      imageAlt: 'Petoro prijatelja sa planinarskim rancima pravi zajedničku fotografiju u šumi',
      requiresLogin: true,
    },
    {
      id: 'purchases',
      title: 'Novčanik i plaćanja',
      description:
        'Upoznaj mogućnosti novčanika i upravljanja zahtevima za dopunu i povlačenje sredstava.',
      action: 'Istraži plaćanja',
      guestAction: 'Prijavi se za plaćanja',
      status: 'available',
      route: '/payment',
      message: 'Funkcionalnosti plaćanja trenutno nisu dostupne za vašu korisničku ulogu.',
      image: 'modern-experiences.webp',
      imageAlt: 'Dve osobe veslaju u kajaku pored šumovitih litica',
      requiresLogin: true,
    },
  ];

  protected readonly tours = this.capabilities[0];
  protected readonly heroMessage = signal('');
  protected readonly paymentMessage = signal('');
  protected readonly paymentRoute = computed(() => {
    const user = this.auth.user();
    if (!user) return '/login';
    if (user.roles.includes('administrator')) return '/payment/admin/requests';
    if (user.roles.includes('explorer')) return '/payment';
    return null;
  });

  protected readonly slides = [
    {
      image: 'gallery-places.webp',
      position: 'center 65%',
      alt: 'Istorijska lokacija sa arhitektonskim detaljima',
      label: 'OTKRIJ PRIČU',
      title: 'Svako mesto krije nešto posebno.',
      description:
        'Iza poznatih ulica i znamenitosti kriju se priče koje čekaju da ih otkriješ. Pogledaj svet iz nove perspektive.',
    },
    {
      image: 'gallery-adventure.webp',
      position: 'center 20%',
      alt: 'Put ka novom istraživanju u prirodi',
      label: 'KRENI U ISTRAŽIVANJE',
      title: 'Najlepša otkrića počinju prvim korakom.',
      description:
        'Pronađi inspiraciju za nove puteve, nepoznate lokacije i doživljaje koji svakom putovanju daju poseban smisao.',
    },
    {
      image: 'gallery-together.webp',
      position: 'center 45%',
      alt: 'Prijatelji zajedno provode vreme u prirodi',
      label: 'PODELI DOŽIVLJAJ',
      title: 'Doživljaji su bolji kada ih podeliš.',
      description:
        'Upoznaj perspektive drugih istraživača kroz njihove blogove i pronađi inspiraciju za svoju sledeću avanturu.',
    },
  ];
  protected readonly selectedSlideIndex = signal(0);
  protected readonly galleryMessage = signal('');
  protected readonly userPaused = signal(false);
  private readonly inView = signal(false);
  private readonly pageActive = signal(false);
  private readonly reducedMotion = signal(true);
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    afterNextRender(() => {
      const gallery = this.element.nativeElement.querySelector('.gallery');
      if (!gallery) return;
      const motion = window.matchMedia('(prefers-reduced-motion: reduce)');
      const updateMotion = () => {
        this.reducedMotion.set(motion.matches);
      };
      const updatePage = () => this.pageActive.set(!document.hidden && document.hasFocus());
      updateMotion();
      updatePage();
      const observer = new IntersectionObserver(
        (entries) => this.inView.set(entries[0].intersectionRatio >= 0.15),
        { threshold: 0.15 },
      );
      observer.observe(gallery);
      motion.addEventListener('change', updateMotion);
      document.addEventListener('visibilitychange', updatePage);
      window.addEventListener('focus', updatePage);
      window.addEventListener('blur', updatePage);
      this.destroyRef.onDestroy(() => {
        observer.disconnect();
        motion.removeEventListener('change', updateMotion);
        document.removeEventListener('visibilitychange', updatePage);
        window.removeEventListener('focus', updatePage);
        window.removeEventListener('blur', updatePage);
      });
    });
    // The effect owns one external timer and clears it whenever any pause condition changes.
    effect((onCleanup) => {
      if (
        this.userPaused() ||
        this.reducedMotion() ||
        !this.inView() ||
        !this.pageActive()
      )
        return;
      const timer = window.setInterval(
        () => this.selectedSlideIndex.update((index) => (index + 1) % this.slides.length),
        6000,
      );
      onCleanup(() => window.clearInterval(timer));
    });
  }

  protected selectSlide(index: number): void {
    this.userPaused.set(true);
    const nextIndex = (index + this.slides.length) % this.slides.length;
    this.selectedSlideIndex.set(nextIndex);
    this.galleryMessage.set(
      `${nextIndex + 1} od ${this.slides.length}: ${this.slides[nextIndex].title}`,
    );
  }
}
