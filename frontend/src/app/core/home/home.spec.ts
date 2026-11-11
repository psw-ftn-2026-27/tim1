import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Auth, User } from '../auth/auth';
import { Home } from './home';

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
    vi.stubGlobal('matchMedia', vi.fn(() => motion));
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
      providers: [provideRouter([]), { provide: Auth, useValue: { user } }],
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
    expect(links('.tile.linked')).toEqual(['/login', '/login']);
    expect(links('a')).not.toContain('/exploration');
    expect(links('a')).not.toContain('/social');
  });

  it('offers tours and blogs to members without login or registration', () => {
    user.set({ id: 'member', email: 'member@example.test', roles: ['explorer'] });
    render();

    expect(links('.hero a')).toEqual(['/exploration', '/social']);
    expect(links('.tile.linked')).toEqual(['/exploration', '/social']);
    expect(links('.join a')).toEqual(['/exploration', '/social']);
    expect(links('a')).not.toContain('/login');
    expect(links('a')).not.toContain('/register');
  });

  it('keeps games and purchases informative without nested controls', () => {
    render();

    const planned = element.querySelectorAll('article.tile');
    expect(planned).toHaveLength(2);
    for (const card of planned) {
      expect(card.textContent).toContain('U pripremi');
      expect(card.querySelector('a,button,[tabindex]')).toBeNull();
    }
    expect(element.querySelector('a a,a button')).toBeNull();
    expect(element.querySelector('.join svg')).toBeNull();
    expect(element.querySelector('footer')?.textContent?.trim()).toBe('© Explorer');
  });

  it('waits six seconds and cycles all three complete slides silently', () => {
    render();
    setVisible(true);

    expect(element.querySelectorAll('.slide')).toHaveLength(3);
    advance(5999);
    expect(selectedSlide()).toBe(0);
    advance(1);
    expect(selectedSlide()).toBe(1);
    expect(element.querySelector('.show img')?.getAttribute('src')).toContain('gallery-adventure.webp');
    expect(element.querySelector('.show')?.textContent).toContain('KRENI U ISTRAŽIVANJE');
    expect(element.querySelector('.show h3')?.textContent).toBe('Najlepša otkrića počinju prvim korakom.');
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
