export default function NotAuthorizedPage() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="glass-card p-8 text-center">
        <h1 className="mb-2 text-2xl font-bold text-error">Not Authorized</h1>
        <p className="text-text-secondary">
          You do not have permission to access eGrants. Please contact your
          administrator for access.
        </p>
      </div>
    </div>
  );
}
