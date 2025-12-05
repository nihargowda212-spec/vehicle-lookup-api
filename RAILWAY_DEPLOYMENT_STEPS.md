# Railway Deployment - Step by Step Guide

## 🚀 Deploy Your Vehicle Lookup App to Railway

### Prerequisites
- ✅ GitHub account
- ✅ Your code ready
- ✅ Railway account (free)

---

## Step 1: Push Your Code to GitHub

### If you don't have a GitHub repository yet:

1. **Install Git** (if not installed): https://git-scm.com/downloads

2. **Open terminal in your project folder:**
   ```bash
   cd C:\Users\Admin\Desktop\Details
   ```

3. **Initialize Git repository:**
   ```bash
   git init
   ```

4. **Add all files:**
   ```bash
   git add .
   ```

5. **Create first commit:**
   ```bash
   git commit -m "Initial commit - Vehicle Lookup API"
   ```

6. **Create repository on GitHub:**
   - Go to: https://github.com/new
   - Repository name: `vehicle-lookup-api` (or any name)
   - Description: "ASP.NET Core Vehicle Registration Lookup"
   - Choose: Public or Private
   - **Don't** initialize with README
   - Click "Create repository"

7. **Connect and push:**
   ```bash
   git remote add origin https://github.com/YOUR_USERNAME/vehicle-lookup-api.git
   git branch -M main
   git push -u origin main
   ```
   (Replace `YOUR_USERNAME` with your GitHub username)

---

## Step 2: Sign Up for Railway

1. **Go to Railway:** https://railway.app
2. **Click "Start a New Project"**
3. **Sign up with GitHub** (recommended - easiest way)
   - Click "Login with GitHub"
   - Authorize Railway to access your GitHub account

---

## Step 3: Deploy from GitHub

1. **After logging in, click "New Project"**

2. **Select "Deploy from GitHub repo"**

3. **Choose your repository:**
   - Find `vehicle-lookup-api` (or whatever you named it)
   - Click on it

4. **Railway will automatically:**
   - Detect it's a .NET project
   - Start building
   - Deploy your application

5. **Wait for deployment** (usually 2-5 minutes)

---

## Step 4: Configure Environment Variables

1. **Go to your project** on Railway dashboard

2. **Click on your service** (should show "vehicle-lookup-api" or similar)

3. **Click "Variables" tab**

4. **Add environment variables:**
   - Click "+ New Variable"
   - **Variable Name:** `RAPIDAPI_KEY`
   - **Value:** `54f47daecemsh7ab18423f71f4e3p1dbe48jsn657e9b1ecc54`
   - Click "Add"

5. **Optional variables:**
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `PORT` = (Railway sets this automatically, but you can override)

---

## Step 5: Get Your Live URL

1. **Go to "Settings" tab** in your Railway project

2. **Click "Generate Domain"** (or Railway will auto-generate one)

3. **Your app will be live at:**
   - `https://your-app-name.railway.app`
   - Or custom domain if you add one

4. **Copy the URL** and test it in your browser!

---

## Step 6: Test Your Live App

1. **Open your Railway URL** in browser
2. **Enter a vehicle registration number**
3. **Click "Search"**
4. **It should work!** 🎉

---

## 🔧 Troubleshooting

### Build Fails?
- Check Railway logs: Dashboard → Deployments → View logs
- Make sure `Program.cs` and `VehicleLookup.csproj` are in root directory
- Verify .NET 8 SDK is available (Railway auto-detects)

### API Not Working?
- Verify `RAPIDAPI_KEY` environment variable is set
- Check Railway logs for API errors
- Make sure you're subscribed to the API on RapidAPI

### App Not Starting?
- Check Railway logs
- Verify environment variables are set correctly
- Make sure PORT environment variable is used (Railway sets this automatically)

---

## 📝 Important Files for Railway

Railway will automatically detect and use:
- ✅ `VehicleLookup.csproj` - Project file
- ✅ `Program.cs` - Main application
- ✅ `appsettings.json` - Configuration (but use env vars for secrets)
- ✅ `wwwroot/` - Static files

**No additional configuration needed!** Railway auto-detects .NET projects.

---

## 🎯 Quick Checklist

- [ ] Code pushed to GitHub
- [ ] Railway account created
- [ ] Project created on Railway
- [ ] GitHub repo connected
- [ ] Environment variable `RAPIDAPI_KEY` set
- [ ] Deployment successful
- [ ] Live URL working
- [ ] Tested vehicle lookup

---

## 💡 Pro Tips

1. **Auto-Deploy:** Every git push automatically redeploys
2. **Logs:** Monitor your app in real-time via Railway dashboard
3. **Custom Domain:** Add your own domain in Settings
4. **Scaling:** Upgrade plan if you need more resources
5. **Free Tier:** $5/month credit is enough for small apps

---

## 🎉 You're Live!

Once deployed, your app will be:
- ✅ Accessible worldwide
- ✅ HTTPS enabled automatically
- ✅ Auto-deploys on every git push
- ✅ Free tier (no credit card needed)

**Your live URL:** `https://your-app-name.railway.app`

---

## 📚 Railway Resources

- Railway Dashboard: https://railway.app/dashboard
- Railway Docs: https://docs.railway.app
- Support: https://railway.app/help

---

**That's it! Your app is now live on Railway! 🚀**

