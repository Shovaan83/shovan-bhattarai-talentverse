import { ReactNode } from "react";

interface AuthLayoutProps {
  children?: ReactNode;
}

export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <div className="min-h-screen bg-[#064E3B] flex items-center justify-center p-4 sm:p-6 lg:p-8 font-sans">
      {/* Card Container */}
      <div className="w-full max-w-[1100px] grid lg:grid-cols-2 bg-white rounded-[30px] overflow-hidden shadow-2xl min-h-[650px]">
        
        {/* Left Side - Hero Image */}
        <div className="hidden lg:block relative bg-gray-900">
          <img
            src="https://images.unsplash.com/photo-1542744173-8e7e53415bb0?q=80&w=1000&auto=format&fit=crop"
            alt="Workspace"
            className="absolute inset-0 w-full h-full object-cover opacity-90"
          />
        </div>

        {/* Right Side - Form Content */}
        <div className="p-8 lg:p-16 flex flex-col justify-center relative">
           <div className="w-full max-w-md mx-auto">
            {children}
           </div>
        </div>
      </div>
    </div>
  );
}
