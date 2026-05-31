interface StatusBadgeProps {
  status: string;
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  let bgColor = 'bg-gray-100 text-gray-800';

  if (status === 'Completed') bgColor = 'bg-green-100 text-green-800';
  if (status === 'In Progress') bgColor = 'bg-blue-100 text-blue-800';
  if (status === 'Pending') bgColor = 'bg-yellow-100 text-yellow-800';

  return (
    <span className={`px-2.5 py-0.5 text-xs font-medium uppercase tracking-wide ${bgColor}`}>
      {status}
    </span>
  );
}