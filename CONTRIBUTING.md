# Contributing to **TryAtSoftware.CleanTests**

Thank you for your interest in contributing to **TryAtSoftware.CleanTests**!
We welcome contributions from the community to help improve our project.
As a part of this document you can find some guidelines to ensure a smooth collaboration.

## How to Contribute

### Reporting Issues
If you encounter any problems, want to suggest improvements or new features, please open an issue.
There are two standard templates for `Bug report` and `Feature request` that you can use.

### Submitting Pull Requests
We encourage you to contribute to **TryAtSoftware.CleanTests** by submitting pull requests.
Follow these steps to submit a PR:
1. Fork the repository to your own GitHub account;
2. Create a new branch: `git checkout -b your-branch-name`;
3. Make your desired changes, adhering to the [guidelines and best practices](#guidelines-and-best-practices);
4. Commit your changes with clear and descriptive messages;
5. Push your branch to your forked repository: `git push origin your-branch-name`;
6. Open a new PR on our GitHub repository with a clear title and detailed description of your changes;
7. Engage in any follow-up discussions or requested changes related to your PR;
8. Once approved, your changes will be merged into the main project.
Note: By submitting a PR, you agree that your code will be licensed under the project's open-source license.

## Guidelines and Best Practices
### Coding Style

We prioritize maintaining high code quality.
In order to achieve this we use SonarCloud.

Please, keep in mind the following guidelines:
- Sonar analyzers are integrated into our development process to identify and report issues such as code smells, bugs, vulnerabilities, and security risks;
- SonarCloud checks are automatically performed on our continuous integration (CI) pipeline, and the build may fail if certain quality or security criteria are not met;
- Be proactive in resolving any reported issues related to code quality, maintainability, and security.

### Testing

We strive to maintain good test coverage to ensure the stability of the project.
When contributing new features or bug fixes, appropriate tests **must** be included as well.
SonarCloud is responsible to measure code coverage and if it is unsatisfactory, that will cause the build to fail.

### Documentation
Clear and comprehensive documentation is crucial for the project's users and contributors. When making changes, update the documentation accordingly.
This includes XML documentation, README files, Wiki pages, and any other relevant documentation.

Please, keep in mind the following guidelines:
- Provide examples and explanations to help users understand how to use the project;
- Document any changes in behavior or configuration that might affect users;
- Use clear and concise language.

## Code of Conduct
We expect all contributors to follow our [Code of Conduct](https://github.com/TryAtSoftware/CleanTests/blob/main/CODE_OF_CONDUCT.md) when participating in our project.

## License
By contributing to this project, you agree that your contributions will be licensed under the project's LICENSE file.
