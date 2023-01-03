using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Biglead.Autoinbox
{
    public partial class AutoSendSms : Form
    {
        List<string> lstImages = new List<string>();
        List<string> lstContents = new List<string>();
        List<string> lstUId = new List<string>();
        ChromeDriver driver;
        public AutoSendSms()
        {
            InitializeComponent();
        }
        private void AutoSendSms_Load(object sender, EventArgs e)
        {
            imgLogo.ImageLocation = "https://cdn.tima.vn/img-web/icons/logo-biglead.jpg"; //path to image
            imgLogo.SizeMode = PictureBoxSizeMode.AutoSize;

            #region Cập nhập phiên bản
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            txtVersion.Text = "Version: " + version;
            #endregion
        }
        private void AutoSendSMS_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát chương trình", "Close", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                CloseSelenium(driver);
                e.Cancel = true;
            }
        }
        private void llbBiglead_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            llbBiglead.LinkVisited = true;
            System.Diagnostics.Process.Start("https://biglead.live/");
        }
        private void btnUploadImage_Click(object sender, EventArgs e)
        {
            lstImages = new List<string>();
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png;)|*.jpg; *.jpeg; *.gif; *.bmp; *.png;";
            if (open.ShowDialog() == DialogResult.OK)
            {
                open.FileNames.ToList().ForEach(file =>
                {
                    lstImages.Add(file);
                });
                ShowNotify("Tải ảnh lên thành công", (int)TypeNotify.Success);
            }
        }
        private void btnClearn_Click(object sender, EventArgs e)
        {
            txtUserName.Text = string.Empty;
            txtPassword.Text = string.Empty;
            rdFanpageOld.Checked = false;
            rdFanpagePro5.Checked = false;
            txtLinkFanpage.Text = string.Empty;
            txtDate.Text = string.Empty;
            lstImages = new List<string>();
            lstContents = new List<string>();
            txtContent.Text = string.Empty;
            txtUserName.Focus();
            ShowNotify("Reset Form thành công", (int)TypeNotify.Success);
        }
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            var userName = txtUserName.Text.Trim();
            var passWord = txtPassword.Text.Trim();
            var typeFanpage = rdFanpageOld.Checked ? (int)TypeFanpage.FanpageOld : rdFanpagePro5.Checked ? (int)TypeFanpage.FanpagePro5 : 0;
            var linkFanpage = txtLinkFanpage.Text.Trim();
            var date = txtDate.Text.Trim();
            var content = txtContent.Text.Trim();
            var amountInbox = txtNumberInbox.Text.Trim();
            DateTime searchDate = DateTime.MinValue;

            #region Validate input
            if (string.IsNullOrEmpty(userName))
                ShowNotify("Vui lòng nhập tài khoản", (int)TypeNotify.Error);
            else if (string.IsNullOrEmpty(passWord))
                ShowNotify("Vui lòng nhập mật khẩu", (int)TypeNotify.Error);
            else if (typeFanpage == 0)
                ShowNotify("Vui lòng chọn loại Fanpage", (int)TypeNotify.Error);
            else if (string.IsNullOrEmpty(linkFanpage))
                ShowNotify("Vui lòng nhập Link Fanpage", (int)TypeNotify.Error);
            else if (string.IsNullOrEmpty(content))
                ShowNotify("Vui lòng nhập nội dung muốn gửi", (int)TypeNotify.Error);
            else if (!string.IsNullOrEmpty(date) && date.Length != 10)
                ShowNotify("Thời gian sai định dạng", (int)TypeNotify.Error);
            else if (string.IsNullOrEmpty(amountInbox))
                ShowNotify("Vui lòng nhập số lượng tin nhắn muốn gửi", (int)TypeNotify.Error);
            else if (!IsNumber(amountInbox))
                ShowNotify("Vui lòng nhập số lượng tin nhắn muốn gửi là số", (int)TypeNotify.Error);
            #endregion
            else
            {
                if (!string.IsNullOrEmpty(date))
                {
                    try
                    {
                        CultureInfo vn = new CultureInfo("vi-VN");
                        searchDate = Convert.ToDateTime(date, vn.DateTimeFormat);
                    }
                    catch (Exception ex)
                    {
                        ShowNotify("Thời gian sai định dạng", (int)TypeNotify.Error);
                    }
                }

                if (content.Contains("|"))
                {
                    lstContents = new List<string>();
                    lstContents = content.Split('|').ToList();
                }
                driver = StartSelenium();
                driver.Navigate().GoToUrl("https://facebook.com/");
                driver.FindElement(By.Id("email")).SendKeys(userName);
                driver.FindElement(By.Id("pass")).SendKeys(passWord);
                driver.FindElement(By.Name("login")).Click();
                try
                {
                    Thread.Sleep(new Random().Next(1000, 4000));
                    if (driver.Url.Contains("privacy_mutation_token"))
                        ShowNotify("Sai tài khoản hoặc mật khẩu", (int)TypeNotify.Error);
                    else
                    {
                        if (driver.Url.Contains("checkpoint"))
                        {
                            ShowNotify("Bảo mật 2 lớp, nhập mã vào Face. Hệ thống dừng 3 phút rồi chạy tiếp", (int)TypeNotify.Error);
                            Thread.Sleep(1000 * 3 * 60); // nghỉ 3 phút rồi cạy tiếp
                        }
                        if (typeFanpage == (int)TypeFanpage.FanpagePro5)
                        {
                            //vào fanpage
                            driver.Navigate().GoToUrl(linkFanpage);
                            Thread.Sleep(new Random().Next(1000, 2000));
                            driver.FindElement(By.ClassName("xwzsa0r")).Click();
                            Thread.Sleep(new Random().Next(1000, 2000));
                            driver.Navigate().GoToUrl("https://business.facebook.com");
                            Thread.Sleep(new Random().Next(1000, 2000));
                            var assetId = CheckPermissionFanpage(driver.Url);
                            driver.Navigate().GoToUrl($"https://business.facebook.com/latest/inbox/all?asset_id={assetId}&nav_ref=bm_home_redirect");
                            Thread.Sleep(new Random().Next(1000, 2000));
                        }
                        else
                        {
                            //kiểm tra xem có quyền vào phần tin nhắn không
                            driver.Navigate().GoToUrl(linkFanpage + "/inbox");
                            var checkPermission = CheckPermissionFanpage(driver.Url);
                            while (true)
                            {
                                if (string.IsNullOrEmpty(checkPermission))
                                    ShowNotify("Bạn không có quyền gửi tin nhắn trong trang. Vui lòng kiểm tra lại", (int)TypeNotify.Error);
                                else
                                    break;
                            }
                        }

                        Actions action = new Actions(driver);
                        var stt = 1;
                        for (int i = 0; i < Convert.ToInt32(amountInbox); i++)
                        {
                            Thread.Sleep(new Random().Next(1000, 3000));
                            lblNotifySend.Text = "Đang gửi tin nhắn ...";
                            var urlCheckUId = driver.Url;
                            var uId = HttpUtility.ParseQueryString(urlCheckUId).Get("selected_item_id");
                            if (!string.IsNullOrEmpty(uId))
                            {
                                if (!lstUId.Contains(uId))
                                {
                                    lstUId.Add(uId);
                                    if (searchDate != DateTime.MinValue)
                                    {
                                        driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                        var strDateSMS = driver.FindElements(By.ClassName("timestamp"))[0].Text;
                                        var dateSMS = DateTime.Now;
                                        if (strDateSMS.Length == 10)
                                        {
                                            CultureInfo vn = new CultureInfo("vi-VN");
                                            dateSMS = Convert.ToDateTime(strDateSMS.Trim(), vn.DateTimeFormat);
                                        }
                                        else
                                            dateSMS = ConvertDateSms(strDateSMS);

                                        if (dateSMS < searchDate)
                                        {
                                            if (lstImages != null && lstImages.Count > 0)
                                            {
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                foreach (var item in lstImages)
                                                {
                                                    Image image = null;
                                                    using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                                    {
                                                        image = Image.FromStream(fs);
                                                    }
                                                    Clipboard.SetImage(image);
                                                    driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                                    Thread.Sleep(new Random().Next(1000, 3000));
                                                    action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").Build().Perform();
                                                    action.KeyUp(OpenQA.Selenium.Keys.Control).Build().Perform();
                                                    Thread.Sleep(new Random().Next(1000, 3000));
                                                }
                                            }
                                            if (lstContents != null && lstContents.Count > 0)
                                            {
                                                foreach (var item in lstContents)
                                                {
                                                    var text = item;
                                                    text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                                    if (text.Contains("##full_name##"))
                                                        text = text.Replace("##full_name##", driver.FindElement(By.ClassName("g2zy4fhw")).Text);
                                                    driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                                    Thread.Sleep(new Random().Next(1000, 3000));
                                                    action.KeyDown(OpenQA.Selenium.Keys.Shift).SendKeys(OpenQA.Selenium.Keys.Enter).Build().Perform();
                                                    action.KeyUp(OpenQA.Selenium.Keys.Shift).Build().Perform();
                                                }
                                            }
                                            else
                                            {
                                                var text = content;
                                                text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                                if (text.Contains("##full_name##"))
                                                    text = text.Replace("##full_name##", driver.FindElement(By.ClassName("g2zy4fhw")).Text);
                                                driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                            }
                                            action.SendKeys(OpenQA.Selenium.Keys.Enter + "").Build().Perform();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            driver.FindElements(By.ClassName("_1t0w"))[0].Click();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                            action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            driver.FindElements(By.ClassName("_4k8w"))[0].Click();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            continue;
                                        }
                                        else
                                        {
                                            driver.FindElements(By.ClassName("_1t0w"))[0].Click();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                            action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            driver.FindElements(By.ClassName("_4k8w"))[0].Click();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            continue;
                                        }
                                    }
                                    if (lstImages != null && lstImages.Count > 0)
                                    {
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                        foreach (var item in lstImages)
                                        {
                                            Image image = null;
                                            using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                            {
                                                image = Image.FromStream(fs);
                                            }
                                            Clipboard.SetImage(image);
                                            driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").Build().Perform();
                                            action.KeyUp(OpenQA.Selenium.Keys.Control).Build().Perform();
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                        }
                                    }
                                    if (lstContents != null && lstContents.Count > 0)
                                    {
                                        foreach (var item in lstContents)
                                        {
                                            var text = item;
                                            text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                            if (text.Contains("##full_name##"))
                                                text = text.Replace("##full_name##", driver.FindElement(By.ClassName("xwz9qih")).Text);
                                            driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                            action.KeyDown(OpenQA.Selenium.Keys.Shift).SendKeys(OpenQA.Selenium.Keys.Enter).Build().Perform();
                                            action.KeyUp(OpenQA.Selenium.Keys.Shift).Build().Perform();
                                        }
                                    }
                                    else
                                    {
                                        var text = content;
                                        text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                        if (text.Contains("##full_name##"))
                                            text = text.Replace("##full_name##", driver.FindElement(By.ClassName("xwz9qih")).Text);
                                        driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                    }

                                    action.SendKeys(OpenQA.Selenium.Keys.Enter + "").Build().Perform();
                                    Thread.Sleep(new Random().Next(1000, 3000));
                                    driver.FindElements(By.ClassName("_1t0w"))[0].Click();
                                    Thread.Sleep(new Random().Next(1000, 3000));
                                    action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                    action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                    Thread.Sleep(new Random().Next(1000, 3000));
                                    driver.FindElements(By.ClassName("_4k8w"))[0].Click();
                                    Thread.Sleep(new Random().Next(1000, 3000));
                                    lblNotifySend.Text = "Đã gửi " + stt++ + " tin nhắn";
                                }
                                else
                                {
                                    for (int j = 1; j <= 9; j++)
                                    {
                                        lstUId.Add(uId);
                                        driver.FindElements(By.ClassName("_4k8w"))[j].Click();
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                        if (searchDate != DateTime.MinValue)
                                        {
                                            driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                            var strDateSMS = driver.FindElements(By.ClassName("timestamp"))[0].Text;
                                            var dateSMS = DateTime.Now;

                                            if (strDateSMS.Length != 9)
                                                dateSMS = ConvertDateSms(strDateSMS);
                                            if (dateSMS < searchDate)
                                            {
                                                if (lstImages != null && lstImages.Count > 0)
                                                {
                                                    Thread.Sleep(new Random().Next(1000, 3000));
                                                    foreach (var item in lstImages)
                                                    {
                                                        Image image = null;
                                                        using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                                        {
                                                            image = Image.FromStream(fs);
                                                        }
                                                        Clipboard.SetImage(image);
                                                        driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                                        Thread.Sleep(new Random().Next(1000, 3000));
                                                        action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").Build().Perform();
                                                        action.KeyUp(OpenQA.Selenium.Keys.Control).Build().Perform();
                                                        Thread.Sleep(new Random().Next(1000, 3000));
                                                    }
                                                }
                                                if (lstContents != null && lstContents.Count > 0)
                                                {
                                                    foreach (var item in lstContents)
                                                    {
                                                        var text = item;
                                                        text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                                        if (text.Contains("##full_name##"))
                                                            text = text.Replace("##full_name##", driver.FindElement(By.ClassName("g2zy4fhw")).Text);
                                                        driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                                        Thread.Sleep(new Random().Next(1000, 3000));
                                                        action.KeyDown(OpenQA.Selenium.Keys.Shift).SendKeys(OpenQA.Selenium.Keys.Enter).Build().Perform();
                                                        action.KeyUp(OpenQA.Selenium.Keys.Shift).Build().Perform();
                                                    }
                                                }
                                                else
                                                {
                                                    var text = content;
                                                    text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                                    if (text.Contains("##full_name##"))
                                                        text = text.Replace("##full_name##", driver.FindElement(By.ClassName("g2zy4fhw")).Text);
                                                    driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                                    Thread.Sleep(new Random().Next(1000, 3000));
                                                }
                                                action.SendKeys(OpenQA.Selenium.Keys.Enter + "").Build().Perform();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                driver.FindElements(By.ClassName("_1t0w"))[0].Click();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                                action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                driver.FindElements(By.ClassName("_4k8w"))[0].Click();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                continue;
                                            }
                                            else
                                            {
                                                driver.FindElements(By.ClassName("_1t0w"))[0].Click();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                                action.SendKeys(OpenQA.Selenium.Keys.Down + "").Build().Perform();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                driver.FindElements(By.ClassName("_4k8w"))[0].Click();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                continue;
                                            }
                                        }
                                        if (lstImages != null && lstImages.Count > 0)
                                        {
                                            foreach (var item in lstImages)
                                            {
                                                Image image = null;
                                                using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                                {
                                                    image = Image.FromStream(fs);
                                                }
                                                Clipboard.SetImage(image);
                                                driver.FindElement(By.ClassName("uiTextareaAutogrow")).Click();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").Build().Perform();
                                                action.KeyUp(OpenQA.Selenium.Keys.Control).Build().Perform();
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                            }
                                        }
                                        if (lstContents != null && lstContents.Count > 0)
                                        {
                                            foreach (var item in lstContents)
                                            {
                                                var text = item;
                                                text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                                if (text.Contains("##full_name##"))
                                                    text = text.Replace("##full_name##", driver.FindElement(By.ClassName("xwz9qih")).Text);

                                                driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                                Thread.Sleep(new Random().Next(1000, 3000));
                                                action.KeyDown(OpenQA.Selenium.Keys.Shift).SendKeys(OpenQA.Selenium.Keys.Enter).Build().Perform();
                                                action.KeyUp(OpenQA.Selenium.Keys.Shift).Build().Perform();
                                            }
                                        }
                                        else
                                        {
                                            var text = content;
                                            text = text.Normalize();//xử lý đưa về bộ unicode utf8
                                            if (text.Contains("##full_name##"))
                                                text = text.Replace("##full_name##", driver.FindElement(By.ClassName("xwz9qih")).Text);
                                            driver.FindElement(By.ClassName("uiTextareaAutogrow")).SendKeys(text.Trim().ToString());
                                            Thread.Sleep(new Random().Next(1000, 3000));
                                        }
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                        action.SendKeys(OpenQA.Selenium.Keys.Enter + "").Build().Perform();
                                        Thread.Sleep(new Random().Next(1000, 3000));
                                        lblNotifySend.Text = "Đã gửi " + stt++ + " tin nhắn";
                                    }
                                }
                            }
                            else continue;
                        }
                        lblNotifySend.Text = "Gửi tin nhắn hoàn tất";
                        CloseSelenium(driver);
                        ShowNotify("Gửi tin nhắn hoàn tất", (int)TypeNotify.Success);
                    }
                }
                catch (Exception ex)
                {
                    ShowNotify("Có lỗi không mong muốn sảy ra " + ex.StackTrace, (int)TypeNotify.Error);
                }
            }
        }
        private ChromeDriver StartSelenium()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--disable-extensions");
            options.AddArguments("--disable-notifications");
            options.AddArguments("--disable-application-cache");
            options.AddArguments("start-maximized");
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var driver = new ChromeDriver(service, options);
            return driver;
        }
        private void CloseSelenium(ChromeDriver driver)
        {
            driver.Close();
            driver.Quit();
            driver.Dispose();
        }
        private void ShowNotify(string message, int type)
        {
            if (type == (int)TypeNotify.Error)
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (type == (int)TypeNotify.Success)
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private string CheckPermissionFanpage(string url)
        {
            var RawUrl = url;
            int index = RawUrl.IndexOf("?");
            return HttpUtility.ParseQueryString(RawUrl).Get("asset_id");
        }
        private DateTime ConvertDateSms(string strDateSMS)
        {
            var dateSMS = DateTime.Now;
            if (strDateSMS == "CN")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
            }
            else if (strDateSMS == "T2")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
            }
            else if (strDateSMS == "T3")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
            }
            else if (strDateSMS == "T4")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
            }
            else if (strDateSMS == "T5")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
            }
            else if (strDateSMS == "T6")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
            }
            else if (strDateSMS == "T7")
            {
                var dateTimeNow = DateTime.Now.DayOfWeek;
                if (dateTimeNow == DayOfWeek.Monday)
                {
                    dateSMS = dateSMS.AddDays(-5);
                }
                else if (dateTimeNow == DayOfWeek.Tuesday)
                {
                    dateSMS = dateSMS.AddDays(-4);
                }
                else if (dateTimeNow == DayOfWeek.Wednesday)
                {
                    dateSMS = dateSMS.AddDays(-3);
                }
                else if (dateTimeNow == DayOfWeek.Thursday)
                {
                    dateSMS = dateSMS.AddDays(-2);
                }
                else if (dateTimeNow == DayOfWeek.Friday)
                {
                    dateSMS = dateSMS.AddDays(-1);
                }
                else if (dateTimeNow == DayOfWeek.Saturday)
                {
                    dateSMS = dateSMS.AddDays(-7);
                }
                else if (dateTimeNow == DayOfWeek.Sunday)
                {
                    dateSMS = dateSMS.AddDays(-6);
                }
            }
            else if (strDateSMS.Trim().ToLower().Contains("tháng"))
            {
                CultureInfo vn = new CultureInfo("vi-VN");
                strDateSMS = strDateSMS.Replace("Tháng", "/").Replace(" ", "").Trim();
                strDateSMS = strDateSMS + "/" + DateTime.Now.Year.ToString();
                dateSMS = Convert.ToDateTime(strDateSMS.Trim(), vn.DateTimeFormat);
            }
            return dateSMS;
        }
        private static bool IsNumber(string pValue)
        {
            foreach (Char c in pValue)
            {
                if (!Char.IsDigit(c))
                    return false;
            }
            return true;
        }
        private enum TypeFanpage
        {
            [Description("Fanpage Cũ")]
            FanpageOld = 1,
            [Description("Fanpage Pro5")]
            FanpagePro5 = 2
        }
        private enum TypeNotify
        {
            [Description("Thành công")]
            Success = 1,
            [Description("Lỗi")]
            Error = -1
        }
    }
}
