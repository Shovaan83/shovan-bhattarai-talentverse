'use client';

import { useState } from 'react';
import { MessageSquare, ArrowLeft } from 'lucide-react';
import { useConversations } from '@/lib/hooks/useMessages';
import { ConversationList } from './components/ConversationList';
import { ChatThread } from './components/ChatThread';
import { useAuth } from '@/lib/hooks/useAuth';

export default function MessagesPage() {
  const [selectedProposalId, setSelectedProposalId] = useState<number | null>(null);
  const { data: conversations = [], isLoading } = useConversations();
  const { user } = useAuth();

  const selectedConversation = conversations.find((c) => c.proposalId === selectedProposalId);

  const handleSelect = (proposalId: number) => {
    setSelectedProposalId(proposalId);
  };

  const handleBack = () => {
    setSelectedProposalId(null);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Page Header (visible on desktop, hidden on mobile when thread is open) */}
        <div className={`mb-6 ${selectedProposalId ? 'hidden md:block' : 'block'}`}>
          <div className="flex items-center gap-3">
            <div className="p-2 bg-emerald-100 rounded-xl">
              <MessageSquare className="w-6 h-6 text-emerald-600" />
            </div>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Messages</h1>
              <p className="text-gray-500 text-sm">Chat with your skill exchange partners</p>
            </div>
          </div>
        </div>

        {/* Split-panel layout */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden" style={{ height: 'calc(100vh - 200px)', minHeight: '500px' }}>
          <div className="flex h-full">
            {/* Left panel - Conversation list */}
            <div
              className={`
                w-full md:w-80 lg:w-96 flex-shrink-0 border-r border-gray-100 flex flex-col
                ${selectedProposalId ? 'hidden md:flex' : 'flex'}
              `}
            >
              {/* Panel header */}
              <div className="px-4 py-3 border-b border-gray-100 bg-gray-50">
                <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
                  Conversations
                  {conversations.length > 0 && (
                    <span className="ml-2 text-gray-400 font-normal normal-case">
                      ({conversations.length})
                    </span>
                  )}
                </p>
              </div>

              {/* List */}
              {isLoading ? (
                <div className="flex-1 flex items-center justify-center">
                  <div className="animate-spin rounded-full h-6 w-6 border-2 border-emerald-500 border-t-transparent" />
                </div>
              ) : (
                <div className="flex-1 overflow-y-auto">
                  <ConversationList
                    conversations={conversations}
                    selectedId={selectedProposalId}
                    onSelect={handleSelect}
                  />
                </div>
              )}
            </div>

            {/* Right panel - Chat thread or empty state */}
            <div
              className={`
                flex-1 flex flex-col min-w-0
                ${selectedProposalId ? 'flex' : 'hidden md:flex'}
              `}
            >
              {selectedConversation && user ? (
                <>
                  {/* Mobile back button */}
                  <div className="md:hidden px-4 py-2 border-b border-gray-100 bg-white">
                    <button
                      onClick={handleBack}
                      className="flex items-center gap-2 text-sm text-gray-600 hover:text-gray-900 transition-colors"
                    >
                      <ArrowLeft className="w-4 h-4" />
                      Back to conversations
                    </button>
                  </div>

                  <ChatThread
                    proposalId={selectedConversation.proposalId}
                    currentUserId={user.id}
                    conversation={selectedConversation}
                  />
                </>
              ) : (
                <div className="flex-1 flex flex-col items-center justify-center text-center p-8">
                  <div className="w-16 h-16 bg-emerald-50 rounded-2xl flex items-center justify-center mb-4">
                    <MessageSquare className="w-8 h-8 text-emerald-400" />
                  </div>
                  <h3 className="font-semibold text-gray-700 text-lg mb-2">Select a conversation</h3>
                  <p className="text-gray-400 text-sm max-w-xs">
                    Choose a conversation from the left to start chatting with your skill exchange partner.
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
