# Arithmetic Calculator Operation API v1.0.0

🚀 We're excited to announce the first official release of the Arithmetic Calculator Operation API!

## Overview

This release delivers a complete serverless microservice for arithmetic operations within the Arithmetic Calculator ecosystem. Built with AWS Lambda and .NET 8, this API provides endpoints for performing basic arithmetic calculations in a scalable, serverless environment.

## Key Features

- ➕ **Arithmetic Operations**
  - Addition, subtraction, multiplication, and division endpoints
  - Input validation and error handling

- 🔒 **Security**
  - Designed for secure, stateless operation
  - Ready for integration with authentication/authorization layers

- 🏗️ **Architecture**
  - Clean Architecture design with separation of concerns
  - Fully serverless implementation with AWS Lambda
  - Containerized deployment options with Docker

## Technical Details

- Built on **.NET 8**
- Implements **Clean Architecture** principles
- **AWS Lambda** serverless deployment
- **CI/CD** pipeline with GitHub Actions

## Getting Started

Check the [README.md](./README.md) for detailed setup instructions, including:

- Local development setup
- Docker deployment
- AWS Lambda deployment
- Environment configuration

## API Endpoints

### Operations
- `POST /v1/operations/add` - Perform addition
- `POST /v1/operations/subtract` - Perform subtraction
- `POST /v1/operations/multiply` - Perform multiplication
- `POST /v1/operations/divide` - Perform division

## Breaking Changes

None (initial release)

## Future Plans

- Support for advanced operations (e.g., square root, exponentiation)
- User authentication and authorization integration
- Operation history and logging
- Performance optimizations
- Multi-language support

---

# Arithmetic Calculator Operation API

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)
![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-FF9900)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)

An API for arithmetic operations, developed using AWS Lambda and .NET 8.

## Features

- **Arithmetic Operations**: Perform addition, subtraction, multiplication, and division
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

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run locally using Lambda Test Tool**
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
   docker run -p 5000:5000 arithmetic-calculator-operation-api
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

### Operations

- **POST /v1/operations/add** - Perform addition
- **POST /v1/operations/subtract** - Perform subtraction
- **POST /v1/operations/multiply** - Perform multiplication
- **POST /v1/operations/divide** - Perform division

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
