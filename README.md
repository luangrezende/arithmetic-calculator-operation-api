# Arithmetic Calculator Operation API

![Build Status](https://img.shields.io/github/actions/workflow/status/luangrezende/arithmetic-calculator-operation-api/ci-cd.yml?branch=main&style=flat-square&logo=github)
![Version](https://img.shields.io/github/v/release/luangrezende/arithmetic-calculator-operation-api?style=flat-square&logo=github)
![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-FF9900?style=flat-square&logo=awslambda&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)

A microservice responsible for arithmetic operations within the Arithmetic Calculator ecosystem. Built on AWS Lambda with .NET 8.

## Features

- **Arithmetic Operations**: Perform addition, subtraction, multiplication, division, and square root
- **Random String Generation**: Generate random alphanumeric strings
- **Serverless Architecture**: Deployed as AWS Lambda functions

## Architecture

This project follows Clean Architecture principles with a clear separation of concerns:

```
├── Domain         - Core business logic and entities
├── Application    - Use cases, DTOs and service interfaces
├── Infrastructure - External concerns (persistence, security)
└── Presentation   - API endpoints and request handling
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) (configured)
- [AWS Lambda Tools for .NET](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools)
  ```bash
  dotnet tool install -g Amazon.Lambda.Tools
  ```
- [Docker](https://www.docker.com/products/docker-desktop) (optional, for containerized development)

## Getting Started

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/luangrezende/arithmetic-calculator-operation-api.git
   cd arithmetic-calculator-operation-api
   ```

2. **Set up environment variables**
   ```bash
   # Windows (PowerShell)
   $env:MYSQL_CONNECTION_STRING="Server=localhost;Database=calculator;User=root;Password=password;"
   $env:JWT_SECRET_KEY="your-jwt-secret-key"
   $env:USER_LAMBDA_BASE_ARN="arn:aws:lambda:us-east-1:123456789012:function:UserAPI"
   $env:USER_DEBIT_ENDPOINT="/v1/user/account/balance"
   $env:USER_PROFILE_ENDPOINT="/v1/user/profile"

   # Linux/macOS
   export MYSQL_CONNECTION_STRING="Server=localhost;Database=calculator;User=root;Password=password;"
   export JWT_SECRET_KEY="your-jwt-secret-key"
   export USER_LAMBDA_BASE_ARN="arn:aws:lambda:us-east-1:123456789012:function:UserAPI"
   export USER_DEBIT_ENDPOINT="/v1/user/account/balance"
   export USER_PROFILE_ENDPOINT="/v1/user/profile"
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run locally using Lambda Test Tool**
   ```bash
   dotnet lambda run-server
   ```

The API will be accessible at `http://localhost:5000`.

### Docker Development

1. **Build the Docker image**
   ```bash
   docker build -t arithmetic-calculator-operation-api .
   ```

2. **Run the container**
   ```bash
   docker run -p 5000:5000 \
     -e MYSQL_CONNECTION_STRING="Server=host.docker.internal;Database=calculator;User=root;Password=password;" \
     -e JWT_SECRET_KEY="your-jwt-secret-key" \
     -e USER_LAMBDA_BASE_ARN="arn:aws:lambda:us-east-1:123456789012:function:UserAPI" \
     -e USER_DEBIT_ENDPOINT="/v1/user/account/balance" \
     -e USER_PROFILE_ENDPOINT="/v1/user/profile" \
     arithmetic-calculator-operation-api
   ```

## Deployment to AWS Lambda

### Manual Deployment

1. **Package the application**
   ```bash
   dotnet lambda package --configuration Release
   ```

2. **Deploy to AWS Lambda**
   ```bash
   dotnet lambda deploy-function ArithmeticCalculatorOperationApi
   ```

### Automated Deployment with GitHub Actions

This project includes a CI/CD pipeline using GitHub Actions. To set up:

1. Add the following secrets to your GitHub repository:
   - `AWS_ACCESS_KEY_ID`
   - `AWS_SECRET_ACCESS_KEY`
   - `LAMBDA_EXECUTION_ROLE_ARN`

2. See [GitHub Actions Setup](docs/github-actions-setup.md) for detailed configuration.

## API Endpoints

### Health Check

- **GET /operation/health** - Health check endpoint for monitoring

### Operations

- **GET /v1/operations/types** - List all operation types
- **GET /v1/operations/records** - List paged operation records  
- **GET /v1/operations/dashboard** - Get dashboard data for the user
- **POST /v1/operations/records** - Create a new operation record (arithmetic or random string)
- **DELETE /v1/operations/records** - Soft delete operation records

## Testing

Run the test suite with:
```bash
cd tests
```
```bash
dotnet test
```

The project includes unit tests for domain, application, and infrastructure layers.

## Configuration

The AWS Lambda configuration is in `aws-lambda-tools-defaults.json`:
```json
{
  "profile": "default",
  "region": "us-east-1",
  "configuration": "Release",
  "framework": "net8.0",
  "function-runtime": "dotnet8",
  "function-memory-size": 256,
  "function-timeout": 30,
  "function-handler": "ArithmeticCalculatorOperationApi.Presentation::ArithmeticCalculatorOperationApi.Presentation.Function::FunctionHandler",
  "function-name": "ArithmeticCalculatorOperationApi",
  "function-description": "Lambda function for Arithmetic Calculator Operation API",
  "package-type": "Zip"
}
```

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [AWS Lambda](https://aws.amazon.com/lambda/)
- [.NET Core](https://dotnet.microsoft.com/)
- [AWS Serverless Application Model](https://aws.amazon.com/serverless/sam/)
