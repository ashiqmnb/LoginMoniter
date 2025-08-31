import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

interface Props {
  children: React.ReactNode;
  role: string;
}

const ProtectedRoute = ({ children, role }: Props) => {
  const userRole = localStorage.getItem("role");
  const navigate = useNavigate();

    useEffect(() => {
      if (userRole !== role) {
      navigate(-1); // Go back to previous page
    }
  }, [userRole, role, navigate]);

  if (userRole !== role) {
    return null; // Prevent rendering children until redirect happens
  }
  return <>{children}</>;
};

export default ProtectedRoute;
