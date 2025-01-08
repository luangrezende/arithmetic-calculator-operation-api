# Arithmetic Calculator Operation API

This is an API for arithmetic operations, developed using AWS Lambda and .NET 8.

## Prerequisites

Ensure the following software is installed on your machine:

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **AWS CLI** - [Installation Instructions](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
3. **AWS Lambda Tools for .NET** - Install using:
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```
4. **Docker** - [Install Docker](https://www.docker.com/products/docker-desktop)

---

## Running Locally

### 1. Clone the Repository

Clone the repository to your local machine:

```bash
git clone https://github.com/luangrezende/arithmetic-calculator-operation-api.git
cd arithmetic-calculator-operation-api
```

### 2. Restore Dependencies

Restore required NuGet packages:

```bash
dotnet restore
```

### 3. Run the API Locally

Run the API locally using the AWS Lambda Test Tool:

```bash
dotnet lambda run-server
```

The API will be accessible at `http://localhost:5000`.

---

## Running with Docker

### 1. Build the Docker Image

Build the Docker image using the following command:

```bash
docker build -t arithmetic-calculator-operation-api .
```

### 2. Run the Docker Container

Run the Docker container:

```bash
docker run -p 5000:5000 arithmetic-calculator-operation-api
```

The API will now be available at `http://localhost:5000`.

---

## Project Structure

```
├── src/
│ ├── ArithmeticCalculatorOperationApi.Presentation/ # Main API project
│ ├── ArithmeticCalculatorOperationApi.Application/ # Application layer (services, DTOs, use cases)
│ ├── ArithmeticCalculatorOperationApi.Domain/ # Domain logic
│ ├── ArithmeticCalculatorOperationApi.Infrastructure/ # Infrastructure logic
├── tests/
│ ├── ArithmeticCalculatorOperationApi.Domain.Tests/ # Unit tests
├── .github/workflows/ # CI/CD workflows
├── .gitignore
├── ArithmeticCalculatorOperationApi.sln # Solution file
└── README.md
```

---

## Configuration

Update the `aws-lambda-tools-defaults.json` file as needed for your Lambda configuration. Example:

```json
{
  "function-name": "ArithmeticCalculatorOperationApi",
  "function-handler": "ArithmeticCalculatorOperationApi::ArithmeticCalculatorOperationApi.Function::FunctionHandler",
  "framework": "net8.0",
  "memory-size": 256,
  "timeout": 30,
  "region": "us-east-1"
}
```

---

## Testing the API

Use tools like **Postman** or **curl** to test the API. Example with `curl`:

```bash
curl -X GET http://localhost:5000/api/operations
```

---

## Resources

- [AWS Lambda for .NET](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet.html)
- [AWS CLI](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-quickstart.html)
- [Docker Documentation](https://docs.docker.com/get-started/)

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.
