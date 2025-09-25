using ArtEShop.Domain.DomainModels;
using ArtEShop.Service.Implementation;
using ArtEShop.Service.Interface;
using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTesting
{
    [TestClass]
    public class FileServiceTests
    {
        private Mock<IWebHostEnvironment> _envMock;
        private IFileService _fileService;
        private string _webRootPath;
        private string[] _allowedExtensions;
        private Mock<IFormFile> _fileMock;
        private string _fileName;
        private string _content;
        private MemoryStream _stream;


        public FileServiceTests()
        {
            _envMock = new Mock<IWebHostEnvironment>();
            _webRootPath = Path.Combine(Path.GetTempPath(), "TestWebRoot");
            _envMock.Setup(e => e.WebRootPath).Returns(_webRootPath);
            _envMock.Setup(e => e.ContentRootPath).Returns(_webRootPath);
            _allowedExtensions = [".jpg", ".png"];
            _fileService = new FileService(_envMock.Object);

            if (!Directory.Exists(_webRootPath))
            {
                Directory.CreateDirectory(_webRootPath);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _fileMock = new Mock<IFormFile>();
            _fileName = "test.jpg";
            _content = "fakeimagecontent";
            _stream = new MemoryStream(Encoding.UTF8.GetBytes(_content));

            _fileMock.Setup(f => f.FileName).Returns(_fileName);
            _fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns<Stream, CancellationToken>((targetStream, token) =>
                    {
                        return _stream.CopyToAsync(targetStream);
                    });
        }

        [TestMethod]
        public async Task SaveFile_Works()
        {
            var savedFileName = await _fileService.SaveFile(_fileMock.Object, _allowedExtensions);
            var expectedPath = Path.Combine(_webRootPath, "uploads", savedFileName);

            Assert.IsTrue(File.Exists(expectedPath));

            File.Delete(expectedPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SaveFile_FileNullException()
        {
            await _fileService.SaveFile(null, _allowedExtensions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SaveFile_NotAllowedExtensionException()
        {
            _fileName = "test.gif";
            _fileMock.Setup(f => f.FileName).Returns(_fileName);
            await _fileService.SaveFile(_fileMock.Object, _allowedExtensions);
        }


        [TestMethod]
        public async Task DeleteFile_Works()
        {
            var expectedPath = Path.Combine(_webRootPath, "uploads", _fileName);
            var savedFileName = await _fileService.SaveFile(_fileMock.Object, _allowedExtensions);
            expectedPath = Path.Combine(_webRootPath, "uploads", savedFileName);

            _fileService.DeleteFile(savedFileName);

            Assert.IsFalse(File.Exists(expectedPath));
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void DeleteFile_FileNotFoundException()
        {
            _fileService.DeleteFile("file.jpg");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteFile_FileIsNullException()
        {
            _fileService.DeleteFile(null);
        }
    }
}
