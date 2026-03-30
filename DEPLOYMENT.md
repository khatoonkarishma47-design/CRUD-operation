# ProductService Deployment Guide

## CI/CD Pipeline

The project uses GitHub Actions for continuous integration and deployment.

### Pipeline Stages

1. **Build** - Compiles the application
2. **Test** - Runs unit tests with code coverage
3. **Code Quality** - Checks code formatting
4. **Security Scan** - Scans for vulnerable packages
5. **Publish** - Creates deployment artifacts (only on main/master branch)

### Triggering the Pipeline

The pipeline runs automatically on:
- Push to `master`, `main`, or `develop` branches
- Pull requests to `master` or `main` branches

## Local Development with Docker

### Prerequisites
- Docker Desktop installed
- Docker Compose installed

### Running Locally

```bash
# Build and start all services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### Service URLs
- **API**: http://localhost:5034
- **Swagger**: http://localhost:5034/swagger
- **Health Check**: http://localhost:5034/health
- **SQL Server**: localhost:1433

## Environment Configuration

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development/Staging/Production) | Yes |
| `JWT_SECRET_KEY` | Secret key for JWT token signing (min 32 chars) | Yes (Production) |
| `DATABASE_CONNECTION_STRING` | SQL Server connection string | Yes (Production) |

### Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Staging.json` - Staging environment
- `appsettings.Production.json` - Production environment

## Manual Deployment

### Building for Production

```bash
# Restore dependencies
dotnet restore

# Build release
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release --output ./publish
```

### Docker Deployment

```bash
# Build Docker image
docker build -t productservice:latest .

# Run container
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  -e JwtSettings__SecretKey="your-secret-key" \
  --name productservice \
  productservice:latest
```

## Health Checks

The application exposes a health check endpoint at `/health` that verifies:
- Application is running
- Database connectivity

## GitHub Actions Secrets

For deployment, configure these secrets in your GitHub repository:

| Secret | Description |
|--------|-------------|
| `JWT_SECRET_KEY` | JWT signing key for production |
| `DATABASE_CONNECTION_STRING` | Production database connection string |
| `DOCKER_USERNAME` | Docker Hub username (if pushing images) |
| `DOCKER_PASSWORD` | Docker Hub password (if pushing images) |

## Extending the Pipeline

To add deployment to a specific platform, add a new job to `.github/workflows/ci-cd.yml`:

### Example: Deploy to Azure App Service

```yaml
deploy-azure:
  name: Deploy to Azure
  runs-on: ubuntu-latest
  needs: [publish]
  if: github.ref == 'refs/heads/main'
  
  steps:
  - name: Download artifacts
    uses: actions/download-artifact@v4
    with:
      name: publish-artifacts
      path: ./publish

  - name: Deploy to Azure Web App
    uses: azure/webapps-deploy@v2
    with:
      app-name: 'your-app-name'
      publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
      package: ./publish
```

### Example: Deploy to AWS ECS

```yaml
deploy-aws:
  name: Deploy to AWS
  runs-on: ubuntu-latest
  needs: [publish]
  if: github.ref == 'refs/heads/main'
  
  steps:
  - name: Configure AWS credentials
    uses: aws-actions/configure-aws-credentials@v4
    with:
      aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
      aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
      aws-region: us-east-1

  - name: Login to Amazon ECR
    id: login-ecr
    uses: aws-actions/amazon-ecr-login@v2

  - name: Build and push Docker image
    run: |
      docker build -t ${{ steps.login-ecr.outputs.registry }}/productservice:${{ github.sha }} .
      docker push ${{ steps.login-ecr.outputs.registry }}/productservice:${{ github.sha }}
```
