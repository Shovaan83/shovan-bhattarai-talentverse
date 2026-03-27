import { chromium } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';

const baseUrl = 'http://localhost:3000';
const outDir = join(process.cwd(), '.qa', 'turn7');
mkdirSync(outDir, { recursive: true });

const routes = [
  '/credits',
  '/credits/success',
  '/credits/cancel',
  '/login',
  '/register',
  '/forgot-password',
  '/setup-2fa',
  '/onboarding',
  '/oauth-callback',
  '/admin',
  '/admin/users',
  '/admin/disputes',
  '/admin/verifications',
  '/admin/moderation',
];

const viewports = [
  { name: 'mobile-375', width: 375, height: 812 },
  { name: 'desktop-1440', width: 1440, height: 900 },
];

const results = [];

function safeName(route) {
  return route.replace(/\//g, '_').replace(/^_/, '') || 'home';
}

const browser = await chromium.launch({ headless: true });

for (const vp of viewports) {
  const context = await browser.newContext({ viewport: { width: vp.width, height: vp.height } });

  for (const route of routes) {
    const page = await context.newPage();
    const consoleErrors = [];
    const requestFailures = [];

    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    page.on('requestfailed', (req) => {
      requestFailures.push(`${req.method()} ${req.url()} :: ${req.failure()?.errorText || 'unknown'}`);
    });

    const url = `${baseUrl}${route}`;
    let status = 'ok';
    let httpStatus = null;

    try {
      const response = await page.goto(url, { waitUntil: 'networkidle', timeout: 45000 });
      httpStatus = response?.status() ?? null;

      await page.waitForTimeout(600);

      const metrics = await page.evaluate(() => {
        const overflowElements = [];
        const all = Array.from(document.querySelectorAll('*'));
        for (const el of all) {
          const rect = el.getBoundingClientRect();
          if (rect.width === 0 || rect.height === 0) continue;
          if (rect.right > window.innerWidth + 1 || rect.left < -1) {
            const tag = el.tagName.toLowerCase();
            const id = el.id ? `#${el.id}` : '';
            const cls = (el.className && typeof el.className === 'string')
              ? `.${el.className.trim().split(/\s+/).slice(0, 2).join('.')}`
              : '';
            overflowElements.push(`${tag}${id}${cls}`);
            if (overflowElements.length >= 8) break;
          }
        }

        return {
          scrollWidth: document.documentElement.scrollWidth,
          innerWidth: window.innerWidth,
          hasHorizontalOverflow: document.documentElement.scrollWidth > window.innerWidth + 1,
          bodyOverflowX: getComputedStyle(document.body).overflowX,
          overflowElements,
          title: document.title,
          h1: document.querySelector('h1')?.textContent?.trim() ?? null,
          path: window.location.pathname,
        };
      });

      const shotPath = join(outDir, `${vp.name}__${safeName(route)}.png`);
      await page.screenshot({ path: shotPath, fullPage: true });

      results.push({
        route,
        viewport: vp.name,
        url,
        finalPath: await page.evaluate(() => window.location.pathname),
        httpStatus,
        status,
        metrics,
        consoleErrors,
        requestFailures,
        screenshot: shotPath,
      });
    } catch (err) {
      status = 'error';
      results.push({
        route,
        viewport: vp.name,
        url,
        status,
        error: String(err),
        consoleErrors,
        requestFailures,
      });
    } finally {
      await page.close();
    }
  }

  await context.close();
}

await browser.close();

const summary = {
  generatedAt: new Date().toISOString(),
  baseUrl,
  totalChecks: results.length,
  failures: results.filter((r) => r.status !== 'ok').length,
  overflowFindings: results
    .filter((r) => r.status === 'ok' && r.metrics?.hasHorizontalOverflow)
    .map((r) => ({ route: r.route, viewport: r.viewport, details: r.metrics })),
  consoleErrorFindings: results
    .filter((r) => r.consoleErrors && r.consoleErrors.length > 0)
    .map((r) => ({ route: r.route, viewport: r.viewport, consoleErrors: r.consoleErrors })),
  requestFailureFindings: results
    .filter((r) => r.requestFailures && r.requestFailures.length > 0)
    .map((r) => ({ route: r.route, viewport: r.viewport, requestFailures: r.requestFailures })),
  redirects: results
    .filter((r) => r.status === 'ok' && r.finalPath && r.finalPath !== r.route)
    .map((r) => ({ route: r.route, viewport: r.viewport, finalPath: r.finalPath })),
  results,
};

const reportPath = join(outDir, 'report.json');
writeFileSync(reportPath, JSON.stringify(summary, null, 2), 'utf-8');

console.log(`Turn 7 visual QA complete. Report: ${reportPath}`);
console.log(`Screenshots: ${outDir}`);
console.log(`Checks: ${summary.totalChecks}, Failures: ${summary.failures}, Overflow findings: ${summary.overflowFindings.length}`);
