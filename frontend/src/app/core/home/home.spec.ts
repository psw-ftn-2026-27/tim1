import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Router, provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Auth, User } from '../auth/auth';
import { Home } from './home';
import { routes } from '../app.routes';

describe('Explorer Home', () => {
  let fixture: ComponentFixture<Home>;
  let element: HTMLElement;
  const user = signal<User | null>(null);
  let observeVisibility: IntersectionObserverCallback;
  let disconnect: ReturnType<typeof vi.fn>;
  let motion: MediaQueryList;
  let focused: boolean;
  let hidden: boolean;

  function render(): void {
    fixture = TestBed.createComponent(Home);
    element = fixture.nativeElement;
    fixture.detectChanges();
    TestBed.tick();
  }

  function setVisible(visible: boolean): void {
    observeVisibility(
      [{ intersectionRatio: visible ? 1 : 0 } as IntersectionObserverEntry],
      {} as IntersectionObserver,
    );
    TestBed.tick();
  }

  function advance(milliseconds: number): void {
    vi.advanceTimersByTime(milliseconds);
    TestBed.tick();
  }

  function selectedSlide(): number {
    return [...element.querySelectorAll('.slide')].findIndex((slide) =>
      slide.classList.contains('show'),
    );
  }

  function links(selector: string): (string | null)[] {
    return [...element.querySelectorAll(selector)].map((link) => link.getAttribute('href'));
  }

  function arrow(direction: 'previous' | 'next'): HTMLButtonElement {
    return element.querySelector<HTMLButtonElement>(
      direction === 'previous' ? '.arrows button:first-child' : '.arrows button:last-child',
    )!;
  }

  beforeEach(() => {
    vi.useFakeTimers({ toFake: ['setInterval', 'clearInterval'] });
    user.set(null);
    focused = true;
    hidden = false;
    motion = Object.assign(new EventTarget(), { matches: false }) as MediaQueryList;
    vi.stubGlobal(
      'matchMedia',
      vi.fn(() => motion),
    );
    vi.spyOn(document, 'hasFocus').mockImplementation(() => focused);
    vi.spyOn(document, 'hidden', 'get').mockImplementation(() => hidden);
    disconnect = vi.fn();
    vi.stubGlobal(
      'IntersectionObserver',
      vi.fn(function (callback: IntersectionObserverCallback) {
        observeVisibility = callback;
        return { observe: vi.fn(), disconnect };
      }),
    );
    TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideRouter(routes), { provide: Auth, useValue: { user } }],
    });
  });

  afterEach(() => {
    fixture?.destroy();
    TestBed.resetTestingModule();
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
    vi.useRealTimers();
  });

  it('offers login for guest exploration and separate registration', () => {
    render();

    expect(links('.hero a')).toEqual(['/login', '/register', '/login']);
    expect(links('.join a')).toEqual(['/register', '/login']);
    expect(links('.tile.linked')).toEqual(['/login', '/login', '/login', '/login']);
    expect(element.querySelector('#payment-status')?.textContent).toBe('');
    expect(links('a')).not.toContain('/exploration');
    expect(links('a')).not.toContain('/social');
  });

  it('offers tours and blogs to members without login or registration', () => {
    user.set({ id: 'member', email: 'member@example.test', roles: ['explorer'] });
    render();

    expect(links('.hero a')).toEqual(['/exploration', '/social']);
    expect(links('.tile.linked')).toEqual(['/exploration', '/games', '/social', '/payment']);
    expect(links('.join a')).toEqual(['/exploration', '/social']);
    expect(links('a')).not.toContain('/login');
    expect(links('a')).not.toContain('/register');
  });

  it('keeps four complete cards without nested controls', () => {
    render();

    const cards = element.querySelectorAll('.tile');
    expect(cards).toHaveLength(4);
    for (const card of cards) {
      expect(card.querySelector('.thumb')).not.toBeNull();
      expect(card.querySelector('.body')).not.toBeNull();
      expect(card.querySelector('a,button,[tabindex]')).toBeNull();
    }
    expect(element.querySelector('a a,a button')).toBeNull();
    expect(element.querySelector('.join svg')).toBeNull();
    expect(element.querySelector('footer')?.textContent?.trim()).toBe('© Explorer');
  });

  it.each([0, 1, 2, 3])(
    'navigates guests to the existing login route from card %i',
    async (index) => {
      render();
      const router = TestBed.inject(Router);
      const navigate = vi.spyOn(router, 'navigateByUrl');

      const card = element.querySelectorAll<HTMLAnchorElement>('.tile')[index];
      card.querySelector<HTMLElement>('.thumb')!.click();
      await navigate.mock.results[0].value;

      expect(router.url).toBe('/login');
      expect(element.querySelector('#payment-status')?.textContent).toBe('');
    },
  );

  it.each([
    [0, '/exploration'],
    [1, '/games'],
    [2, '/social'],
    [3, '/payment'],
  ])('navigates explorers from card %i to the existing %s route', async (index, destination) => {
    user.set({ id: 'member', email: 'member@example.test', roles: ['explorer'] });
    render();
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigateByUrl');

    const card = element.querySelectorAll<HTMLAnchorElement>('.tile')[index];
    card.querySelector<HTMLElement>('.thumb')!.click();
    await navigate.mock.results[0].value;

    expect(router.url).toBe(destination);
  });

  it.each([['administrator'], ['explorer', 'administrator']])(
    'opens wallet administration for roles %j',
    async (...roles) => {
      user.set({ id: 'admin', email: 'admin@example.test', roles });
      render();
      const router = TestBed.inject(Router);
      const navigate = vi.spyOn(router, 'navigateByUrl');

      element.querySelectorAll<HTMLAnchorElement>('.tile')[3].click();
      await navigate.mock.results[0].value;

      expect(router.url).toBe('/payment/admin/requests');
    },
  );

  it.each([[], ['other']])(
    'explains unavailable payment without navigating for roles %j',
    (...roles) => {
      user.set({ id: 'member', email: 'member@example.test', roles });
      render();
      const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl');
      const payment = element.querySelector<HTMLButtonElement>('button.tile')!;
      payment.focus();

      payment.querySelector<HTMLElement>('.thumb')!.click();
      TestBed.tick();

      expect(navigate).not.toHaveBeenCalled();
      expect(TestBed.inject(Router).url).toBe('/');
      expect(document.activeElement).toBe(payment);
      expect(payment.type).toBe('button');
      const status = element.querySelector('#payment-status')!;
      expect(status.getAttribute('role')).toBe('status');
      expect(status.textContent).toBe(
        'Funkcionalnosti plaćanja trenutno nisu dostupne za vašu korisničku ulogu.',
      );
    },
  );

  it('clears the payment notice when Home is destroyed and opened again', () => {
    user.set({ id: 'member', email: 'member@example.test', roles: [] });
    render();
    element.querySelector<HTMLButtonElement>('button.tile')!.click();
    TestBed.tick();
    expect(element.querySelector('#payment-status')?.textContent).not.toBe('');

    fixture.destroy();
    render();

    expect(element.querySelector('#payment-status')?.textContent).toBe('');
    element.querySelector<HTMLButtonElement>('button.tile')!.click();
    TestBed.tick();
    expect(element.querySelector('#payment-status')?.textContent).not.toBe('');
  });

  it('waits six seconds and cycles all three complete slides silently', () => {
    render();
    setVisible(true);

    expect(element.querySelectorAll('.slide')).toHaveLength(3);
    advance(5999);
    expect(selectedSlide()).toBe(0);
    advance(1);
    expect(selectedSlide()).toBe(1);
    expect(element.querySelector('.show img')?.getAttribute('src')).toContain(
      'gallery-adventure.webp',
    );
    expect(element.querySelector('.show')?.textContent).toContain('KRENI U ISTRAŽIVANJE');
    expect(element.querySelector('.show h3')?.textContent).toBe(
      'Najlepša otkrića počinju prvim korakom.',
    );
    advance(6000);
    expect(selectedSlide()).toBe(2);
    advance(6000);
    expect(selectedSlide()).toBe(0);
    expect(element.querySelector('.gallery [role="status"]')?.textContent).toBe('');
  });

  it('continues autoplay while the pointer is over the gallery', () => {
    render();
    setVisible(true);

    element.querySelector('.gallery')!.dispatchEvent(new MouseEvent('mouseenter'));
    advance(6000);

    expect(selectedSlide()).toBe(1);
  });

  it('wraps manual arrows and stays paused after manual selection', () => {
    render();
    setVisible(true);

    arrow('previous').click();
    TestBed.tick();
    expect(selectedSlide()).toBe(2);
    expect(element.querySelector('.gallery [role="status"]')?.textContent).toContain('3 od 3');
    arrow('next').click();
    TestBed.tick();
    expect(selectedSlide()).toBe(0);
    advance(18000);
    expect(selectedSlide()).toBe(0);
    expect(vi.getTimerCount()).toBe(0);
  });

  it('stays paused after keyboard focus leaves the gallery', () => {
    render();
    setVisible(true);

    arrow('next').dispatchEvent(new FocusEvent('focusin', { bubbles: true }));
    TestBed.tick();
    arrow('next').dispatchEvent(new FocusEvent('focusout', { bubbles: true }));
    advance(12000);

    expect(selectedSlide()).toBe(0);
    expect(vi.getTimerCount()).toBe(0);
  });

  it('disables autoplay for reduced motion while preserving manual arrows', () => {
    Object.defineProperty(motion, 'matches', { value: true });
    render();
    setVisible(true);

    advance(12000);
    expect(selectedSlide()).toBe(0);
    expect(vi.getTimerCount()).toBe(0);
    arrow('next').click();
    TestBed.tick();
    expect(selectedSlide()).toBe(1);
  });

  it('responds to reduced motion changes during playback', () => {
    render();
    setVisible(true);

    Object.defineProperty(motion, 'matches', { value: true });
    motion.dispatchEvent(new Event('change'));
    advance(12000);

    expect(selectedSlide()).toBe(0);
    expect(vi.getTimerCount()).toBe(0);
  });

  it('runs only while the gallery is visible', () => {
    render();
    advance(12000);
    expect(selectedSlide()).toBe(0);
    setVisible(true);
    advance(6000);
    expect(selectedSlide()).toBe(1);
    setVisible(false);
    advance(12000);
    expect(selectedSlide()).toBe(1);
    setVisible(true);
    advance(6000);
    expect(selectedSlide()).toBe(2);
  });

  it('pauses in a hidden document and resumes when visible', () => {
    render();
    setVisible(true);

    hidden = true;
    document.dispatchEvent(new Event('visibilitychange'));
    advance(12000);
    expect(selectedSlide()).toBe(0);
    hidden = false;
    document.dispatchEvent(new Event('visibilitychange'));
    TestBed.tick();
    advance(6000);
    expect(selectedSlide()).toBe(1);
  });

  it('pauses when the window loses focus', () => {
    render();
    setVisible(true);

    focused = false;
    window.dispatchEvent(new Event('blur'));
    advance(12000);
    expect(selectedSlide()).toBe(0);
    focused = true;
    window.dispatchEvent(new Event('focus'));
    TestBed.tick();
    advance(6000);
    expect(selectedSlide()).toBe(1);
  });

  it('cleans the timer, observer and all event listeners on destruction', () => {
    render();
    setVisible(true);
    const removeMotion = vi.spyOn(motion, 'removeEventListener');
    const removeDocument = vi.spyOn(document, 'removeEventListener');
    const removeWindow = vi.spyOn(window, 'removeEventListener');

    fixture.destroy();

    expect(vi.getTimerCount()).toBe(0);
    expect(disconnect).toHaveBeenCalledOnce();
    expect(removeMotion).toHaveBeenCalledWith('change', expect.any(Function));
    expect(removeDocument).toHaveBeenCalledWith('visibilitychange', expect.any(Function));
    expect(removeWindow).toHaveBeenCalledWith('focus', expect.any(Function));
    expect(removeWindow).toHaveBeenCalledWith('blur', expect.any(Function));
  });
});
