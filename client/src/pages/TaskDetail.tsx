import { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { type RootState } from '../store/store';
import { tasksService, type TaskItem } from '../api/taskApi';
import StatusBadge from '../components/StatusBadge';

export default function TaskDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { role, userId } = useSelector((state: RootState) => state.auth);

  const [task, setTask] = useState<TaskItem | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSingleTask = async () => {
      try {
        if (!id) return;
        const data = await tasksService.getTaskById(Number(id));
        setTask(data);
      } catch (err) {
        setError('Could not load task details. It may have been deleted.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchSingleTask();
  }, [id]); 

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;
    
    try {
      await tasksService.deleteTask(Number(id));
      navigate('/tasks'); 
    } catch (err) {
      alert('Failed to delete task.');
    }
  };

  if (isLoading) return <div className="p-8 text-center text-gray-500">Loading task details...</div>;
  if (error || !task) return <div className="p-4 bg-red-50 text-red-700 border border-red-200">{error}</div>;

  //  UI PERMISSION LOGIC 
  const isAdmin = role === 'Admin';
  const isCreator = task.creatorUserId === userId;
  const isAssigned = task.assignedUserId === userId;

  // A restricted assignee is someone who is assigned the task, but didn't create it, and isn't an Admin.
  const isRestrictedAssignee = isAssigned && !isCreator && !isAdmin;

  const canEdit = isAdmin || isCreator || isAssigned; 
  const canDelete = isAdmin || isCreator; 

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      
      {/* Back Navigation */}
      <Link to="/tasks" className="text-sm text-blue-600 hover:text-blue-800 font-medium">
        &larr; Back to Task Directory
      </Link>

      {/* heads up about their permissions */}
      {isRestrictedAssignee && (
        <div className="p-4 bg-blue-50 text-blue-800 border border-blue-200 text-sm shadow-sm">
          <strong>Note:</strong> You are the assignee for this task. You have permission to update its status to keep the team informed.
        </div>
      )}

      <div className="bg-white border border-gray-200 p-6 sm:p-8 shadow-sm">
        
        {/* Header Section */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6 pb-6 border-b border-gray-200">
          <div>
            <h1 className="text-2xl font-semibold text-gray-900">{task.title}</h1>
            <p className="text-sm text-gray-500 mt-1">Task ID: #{task.id}</p>
          </div>
          <StatusBadge status={task.status} />
        </div>

        {/* Details Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 mb-8">
          <div>
            <p className="text-sm font-medium text-gray-500 mb-1">Category</p>
            <p className="text-gray-900">{task.category || 'Uncategorized'}</p>
          </div>
          <div>
            <p className="text-sm font-medium text-gray-500 mb-1">Priority</p>
            <p className="text-gray-900">{task.priority}</p>
          </div>
          <div>
            <p className="text-sm font-medium text-gray-500 mb-1">Due Date</p>
            <p className="text-gray-900">
              {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No Due Date'}
            </p>
          </div>
          <div>
            <p className="text-sm font-medium text-gray-500 mb-1">Assigned To</p>
            <p className="text-gray-900 truncate" title={task.assignedUserId}>
              {task.assignedUserName ? task.assignedUserName : 'Unassigned'}
            </p>
          </div>
        </div>

        {/* Description Section */}
        <div className="mb-8">
          <p className="text-sm font-medium text-gray-500 mb-2">Description</p>
          <div className="bg-gray-50 p-4 border border-gray-200 text-gray-800 whitespace-pre-wrap min-h-[100px]">
            {task.description || 'No description provided.'}
          </div>
        </div>

        {/* Action Buttons */}
        {(canEdit || canDelete) && (
          <div className="flex gap-3 pt-6 border-t border-gray-200">
            {canEdit && (
              <Link 
                to={`/tasks/${task.id}/edit`}
                className="bg-white border border-gray-300 text-gray-700 px-4 py-2 text-sm font-medium hover:bg-gray-50 transition-colors"
              >
                {/* DYNAMIC BUTTON TEXT */}
                {isRestrictedAssignee ? 'Update Status' : 'Edit Task'}
              </Link>
            )}
            {canDelete && (
              <button 
                onClick={handleDelete}
                className="bg-white border border-red-300 text-red-600 px-4 py-2 text-sm font-medium hover:bg-red-50 transition-colors ml-auto"
              >
                Delete
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}