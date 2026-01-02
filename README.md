# KeyZee SDK
[![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge)](LICENSE)

## Table Of Contents
- [Overview](#overview)
- [Contributing](#contributing)

## Overview

KeyZee is a lightweight, self-hostable encrypted key-value pair store built with .NET. It provides a secure way to store sensitive configuration data, secrets, and key-value pairs with built-in AES encryption. KeyZee is designed following Clean Architecture principles, making it maintainable, testable, and easily extensible.

## Contributing

KeyZee is **free and open-source software** licensed under the MIT License. You're welcome to use it for both personal and commercial projects at no cost.

### We Welcome Contributions! 🎉

Contributions are always welcome and appreciated! Whether it's:

- 🐛 **Bug reports** - Found a bug? Please open an issue with steps to reproduce
- 💡 **Feature requests** - Have an idea? Share it in the issues section
- 📖 **Documentation improvements** - Help make the docs clearer
- 🔧 **Code contributions** - Submit a pull request with your improvements
- ⭐ **Starring the repo** - Show your support!

### How to Contribute

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes** and commit (`git commit -m 'Add amazing feature'`)
4. **If database schema changed**, run migrations script:
   - Linux/Mac: `./add-migrations.sh`
   - Windows: `./add-migrations.ps1`
5. **Push to your branch** (`git push origin feature/amazing-feature`)
6. **Open a Pull Request** describing your changes

### Guidelines

- Follow existing code style and conventions
- Write clear commit messages
- Add tests for new features when applicable
- Update documentation as needed
- Be respectful and constructive in discussions

### Development Setup

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/keyzee-sdk.git
cd keyzee-sdk/keyzee

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run locally
dotnet run -- app list
```

### Support & Maintenance

This project is maintained by [Mark Hart](https://github.com/MegaByteMark) in my spare time. While I do my best to respond to issues and review pull requests promptly, please be patient - I may not be able to respond immediately.

If you find this project useful, consider:
- ⭐ Starring the repository
- 📣 Sharing it with others
- 💬 Providing feedback and suggestions

### Code of Conduct

Please be kind, respectful, and professional. We're all here to learn and build something useful together.

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. This means you can:
- ✅ Use it commercially
- ✅ Modify it
- ✅ Distribute it
- ✅ Use it privately

The only requirement is to include the original copyright notice.

---

**Questions?** Feel free to open an issue or start a discussion. Happy coding! 🚀
