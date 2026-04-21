"use client";

import { ShieldCheck, LogOut, Settings as SettingsIcon } from "lucide-react";
import { accountApi } from "@/lib/api/account";
import LinkedAccountsSettings from "@/app/profile/components/LinkedAccountsSettings";
import VerificationRequestForm from "@/app/profile/components/VerificationRequestForm";

export default function SettingsPage() {
  return (
    <div className="min-h-screen p-4 md:p-8 bg-[#FAFAFA] text-zinc-900">
      <div className="max-w-5xl mx-auto">
        <header className="mb-6">
          <h1 className="text-2xl font-display font-bold text-zinc-900 flex items-center gap-2">
            <SettingsIcon className="w-6 h-6" />
            Settings
          </h1>
          <p className="text-zinc-600 font-body text-sm mt-1">
            Manage linked accounts, verification, and your active session.
          </p>
        </header>

        <div className="space-y-6">
          <section className="bg-white border border-zinc-200 rounded-xl p-6">
            <LinkedAccountsSettings />
          </section>

          <section className="bg-white border border-zinc-200 rounded-xl p-6">
            <div className="flex items-center gap-2 mb-2">
              <ShieldCheck className="w-5 h-5 text-zinc-700" />
              <h2 className="text-lg font-semibold text-zinc-900">Identity Verification</h2>
            </div>
            <p className="text-sm text-zinc-600 mb-4">
              Get verified to increase trust on the platform and earn a special badge plus 25 credits.
            </p>
            <VerificationRequestForm />
          </section>

          <section className="bg-white border border-zinc-200 rounded-xl p-6">
            <div className="flex items-center gap-2 mb-2">
              <LogOut className="w-5 h-5 text-red-600" />
              <h2 className="text-lg font-semibold text-zinc-900">Session Management</h2>
            </div>
            <p className="text-sm text-zinc-600 mb-4">
              Securely log out of your account. This revokes your refresh token.
            </p>
            <button
              onClick={() => accountApi.logout()}
              className="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg font-medium text-sm transition-colors"
            >
              Logout
            </button>
          </section>
        </div>
      </div>
    </div>
  );
}
