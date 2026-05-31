import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { tasksService, type TaskItem } from '../api/taskApi';
import StatusBadge from '../components/StatusBadge';

export default function TaskList() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
 
  const [filterStatus, setFilterStatus] = useState<string>('All');

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        const data = await tasksService.getAllTasks();
        setTasks(data);
      } catch (err) {
        setError('Failed to load tasks. Please try again.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchTasks();
  }, []);

  // Filter the tasks before rendering them
  const displayedTasks = filterStatus === 'All' 
    ? tasks 
    : tasks.filter(t => t.status === filterStatus);

  if (isLoading) return <div className="p-8 text-center text-gray-500">Loading tasks...</div>;
  if (error) return <div className="p-4 bg-red-50 text-red-700 border border-red-200">{error}</div>;

  return (
    <div className="space-y-6">
      
      {/* Header & Controls */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-end gap-4 border-b border-gray-200 pb-4">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Task Directory</h1>
          <p className="text-sm text-gray-500 mt-1">Manage and track all project tasks.</p>
        </div>
        
        <div className="flex items-center gap-4">
          <select 
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="border border-gray-300 py-2 px-3 text-sm focus:outline-none focus:ring-1 focus:ring-blue-600 focus:border-blue-600 bg-white"
          >
            <option value="All">All Statuses</option>
            <option value="Pending">Pending</option>
            <option value="In Progress">In Progress</option>
            <option value="Completed">Completed</option>
          </select>
          
          <Link 
            to="/tasks/new" 
            className="bg-blue-600 text-white px-4 py-2 text-sm font-medium hover:bg-blue-700 transition-colors"
          >
            + New Task
          </Link>
        </div>
      </div>

      {/* The Data Table */}
      <div className="bg-white border border-gray-200 overflow-x-auto">
        <table className="w-full text-left text-sm whitespace-nowrap">
          <thead className="bg-gray-50 border-b border-gray-200 text-gray-500 uppercase tracking-wider">
            <tr>
              <th className="px-6 py-3 font-medium">Task Title</th>
              <th className="px-6 py-3 font-medium">Category</th>
              <th className="px-6 py-3 font-medium">Priority</th>
              <th className="px-6 py-3 font-medium">Due Date</th>
              <th className="px-6 py-3 font-medium">Status</th>
              
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {displayedTasks.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                  No tasks found matching your criteria.
                </td>
              </tr>
            ) : (
              displayedTasks.map((task) => (
                <tr key={task.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 font-medium text-gray-900">
                    {task.title}
                  </td>
                  <td className="px-6 py-4 text-gray-500">
                    {task.category || 'N/A'}
                  </td>
                  <td className="px-6 py-4 text-gray-500">
                    {task.priority}
                  </td>
                  <td className="px-6 py-4 text-gray-500">
                    {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No Date'}
                  </td>
                  <td className="px-6 py-4">
                    <StatusBadge status={task.status} />
                  </td>
                  <td className="px-6 py-4 text-right">
                    <Link 
                      to={`/tasks/${task.id}`}
                      className="text-blue-600 hover:text-blue-800 font-medium text-sm"
                    >
                      View &rarr;
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}