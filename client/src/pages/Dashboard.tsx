import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { type RootState } from '../store/store';
import { tasksService, type TaskItem } from '../api/taskApi';
import StatCard from '../components/StatCard';

export default function Dashboard() {

  const { role, userId } = useSelector((state: RootState) => state.auth);
  
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
      
        const data = await tasksService.getAllTasks();
        setTasks(data);
      } catch (err) {
        setError('Failed to load dashboard data. Please try again later.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  if (isLoading) {
    return (
      <div className="flex justify-center items-center h-64 text-gray-500">
        Loading dashboard...
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 text-red-700">
        {error}
      </div>
    );
  }

  
  // If the user is a regular user only count their tasks. If Admin count all tasks.
  const relevantTasks = role === 'Admin' 
    ? tasks 
    : tasks.filter(t => t.assignedUserId === userId);

  const totalTasks = relevantTasks.length;
  const completedTasks = relevantTasks.filter(t => t.status === 'Completed').length;
  const pendingTasks = relevantTasks.filter(t => t.status === 'Pending').length;
  const inProgressTasks = relevantTasks.filter(t => t.status === 'In Progress').length;

  return (
    <div className="space-y-6">
      
      {/* Page Header */}
      <div className="flex justify-between items-end border-b border-gray-200 pb-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Dashboard</h1>
          <p className="text-sm text-gray-500 mt-1">
            {role === 'Admin' ? 'Company-wide task overview.' : 'Your personal task overview.'}
          </p>
        </div>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Total Tasks" value={totalTasks} />
        <StatCard title="Completed" value={completedTasks} />
        <StatCard title="In Progress" value={inProgressTasks} />
        <StatCard title="Pending" value={pendingTasks} />
      </div>

      
    </div>
  );
}