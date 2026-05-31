interface StatCardProps {
  title: string;
  value: number | string;
  icon?: React.ReactNode; 
}

export default function StatCard({ title, value, icon }: StatCardProps) {
  return (
    <div className="bg-white p-6 border border-gray-200 shadow-sm flex items-center justify-between">
      <div>
        <p className="text-sm font-medium text-gray-500 uppercase tracking-wider mb-1">
          {title}
        </p>
        <p className="text-3xl font-semibold text-gray-900">
          {value}
        </p>
      </div>
      {/* If we pass an icon in, it will render here in a light gray circle */}
      {icon && (
        <div className="p-3 bg-gray-50 text-gray-400 rounded-full">
          {icon}
        </div>
      )}
    </div>
  );
}