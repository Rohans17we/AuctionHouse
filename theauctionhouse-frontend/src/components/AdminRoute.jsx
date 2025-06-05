import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const AdminRoute = ({ children }) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    // Redirect to admin login if not authenticated
    return <Navigate to="/admin/login" replace />;
  }

  if (!user?.isAdmin) {
    // Redirect to regular dashboard if user is not an admin
    return <Navigate to="/dashboard" replace />;
  }

  return children;
};

export default AdminRoute;
