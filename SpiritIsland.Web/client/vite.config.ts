import { defineConfig } from 'vite';
import preact from '@preact/preset-vite';

// `npm run dev` serves the client with hot reload and forwards /api to the .NET server:
//     dotnet run --project ../SpiritIsland.Web.csproj --urls http://localhost:5080
// `npm run build` writes the finished client into ../wwwroot, which the .NET server serves.
export default defineConfig({
  plugins: [preact()],
  build: { outDir: '../wwwroot', emptyOutDir: true },
  server: { proxy: { '/api': 'http://localhost:5080' } },
});
