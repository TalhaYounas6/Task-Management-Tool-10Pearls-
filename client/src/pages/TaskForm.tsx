import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { type RootState } from '../store/store';
import { tasksService } from '../api/taskApi';
import { usersService, type UserItem } from '../api/usersApi'; 

export default function TaskForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { role, userId } = useSelector((state: RootState) => state.auth);
  
  const isEditMode = Boolean(id);

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [category, setCategory] = useState('Development');
  const [dueDate, setDueDate] = useState('');
  const [status, setStatus] = useState('Pending');
  
  const [assignedUserId, setAssignedUserId] = useState<string>('');
  const [usersList, setUsersList] = useState<UserItem[]>([]);

  const [creatorUserId, setCreatorUserId] = useState<string | null>(null);

  const [isLoading, setIsLoading] = useState(isEditMode || role === 'Admin');
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadData = async () => {
      try {
        // Only fetch the full user list if they are an Admin to save bandwidth & prevent 403 errors
        if (role === 'Admin') {
          const users = await usersService.getAllUsers();
          setUsersList(users);
        }

        if (isEditMode) {
          const taskData = await tasksService.getTaskById(Number(id));
          setTitle(taskData.title);
          setDescription(taskData.description || '');
          setPriority(taskData.priority);
          if (taskData.category) setCategory(taskData.category);
          setStatus(taskData.status);
          if (taskData.assignedUserId) setAssignedUserId(taskData.assignedUserId);
          
          if (taskData.creatorUserId) setCreatorUserId(taskData.creatorUserId);
          
          if (taskData.dueDate) {
            setDueDate(new Date(taskData.dueDate).toISOString().split('T')[0]);
          }
        }
      } catch (err) {
        setError('Failed to load necessary data.');
      } finally {
        setIsLoading(false);
      }
    };

    loadData();
  }, [id, isEditMode, role]);

  const handleSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();
    setIsSaving(true);
    setError(null);

    const finalAssignedUserId = assignedUserId;

    try {
      if (isEditMode) {
        await tasksService.updateTask(Number(id), {
          title,
          description,
          priority,
          category,
          dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
          status,
          assignedUserId: finalAssignedUserId
        });
        navigate(`/tasks/${id}`);
      } else {
        await tasksService.createTask({
          title,
          description,
          priority,
          category,
          dueDate: dueDate ? new Date(dueDate).toISOString() : undefined,
          assignedUserId: finalAssignedUserId
        });
        navigate('/tasks');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'An error occurred while saving.');
      setIsSaving(false);
    }
  };

  if (isLoading) return <div className="p-8 text-center text-gray-500">Loading form...</div>;

  // Lock the form if it is an edit, they are not an admin, AND they did not create it.
  const isRestrictedAssignee = isEditMode && role !== 'Admin' && creatorUserId !== userId;
  
  const inputStyles = `w-full px-3 py-2 border border-gray-300 focus:outline-none focus:ring-1 focus:ring-blue-600 focus:border-blue-600 ${isRestrictedAssignee ? 'bg-gray-100 text-gray-500 cursor-not-allowed' : 'bg-white'}`;

  return (
    <div className="max-w-2xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">
          {isEditMode ? 'Edit Task' : 'Create New Task'}
        </h1>
        <Link to={isEditMode ? `/tasks/${id}` : '/tasks'} className="text-sm text-gray-500 hover:text-gray-900">
          Cancel
        </Link>
      </div>

      {error && (
        <div className="mb-6 p-4 bg-red-50 text-red-700 border border-red-200 text-sm">
          {error}
        </div>
      )}

      {isRestrictedAssignee && (
        <div className="mb-6 p-4 bg-yellow-50 text-yellow-800 border border-yellow-200 text-sm">
          You are assigned to this task but did not create it. You may only update its status.
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-white border border-gray-200 p-6 sm:p-8 shadow-sm space-y-6">
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Task Title *</label>
          <input
            type="text"
            required
            disabled={isRestrictedAssignee}
            maxLength={200}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className={inputStyles}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            rows={4}
            disabled={isRestrictedAssignee}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className={inputStyles}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
            <select
              value={category}
              disabled={isRestrictedAssignee}
              onChange={(e) => setCategory(e.target.value)}
              className={inputStyles}
            >
              <option value="Development">Development</option>
              <option value="Design">Design</option>
              <option value="Testing">Testing</option>
              <option value="Bug">Bug</option>
              <option value="General">General</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
            <select
              value={priority}
              disabled={isRestrictedAssignee}
              onChange={(e) => setPriority(e.target.value)}
              className={inputStyles}
            >
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
            <input
              type="date"
              disabled={isRestrictedAssignee}
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
              className={inputStyles}
            />
          </div>

          {isEditMode && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
              {/*  this select doesn't use inputStyles so it stays editable for restricted assignees! */}
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 focus:outline-none focus:ring-1 focus:ring-blue-600 focus:border-blue-600 bg-white"
              >
                <option value="Pending">Pending</option>
                <option value="In Progress">In Progress</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
          )}

          {/* DYNAMIC ASSIGNMENT UI */}
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">Assign Task To</label>
            
            {role === 'Admin' ? (
              <>
                <select
                  value={assignedUserId}
                  onChange={(e) => setAssignedUserId(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 focus:outline-none focus:ring-1 focus:ring-blue-600 focus:border-blue-600 bg-white"
                >
                  <option value="">Assign to myself</option>
                  {usersList.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.fullName} ({user.email})
                    </option>
                  ))}
                </select>
                <p className="text-xs text-gray-500 mt-1">
                  Leave blank to automatically assign the task to yourself.
                </p>
              </>
            ) : (
              <>
                <input
                  type="text"
                  disabled
                  value={isEditMode && assignedUserId !== userId ? "Another User" : "You (Default)"}
                  className="w-full px-3 py-2 border border-gray-300 bg-gray-100 text-gray-500 cursor-not-allowed"
                />
                {!isEditMode && (
                  <p className="text-xs text-gray-500 mt-1">
                    Standard users can only create tasks assigned to themselves.
                  </p>
                )}
              </>
            )}
          </div>
        </div>

        <div className="pt-4 border-t border-gray-200">
          <button
            type="submit"
            disabled={isSaving}
            className="w-full sm:w-auto bg-blue-600 text-white font-medium py-2 px-6 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-600 focus:ring-offset-2 disabled:bg-blue-400 transition-colors"
          >
            {isSaving ? 'Saving...' : isEditMode ? 'Update Task' : 'Create Task'}
          </button>
        </div>
      </form>
    </div>
  );
}