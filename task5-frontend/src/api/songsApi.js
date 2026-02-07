const BASE_URL = "https://task5-danyloly­siuk-h6btabfvhzaub9et.canadacentral-01.azurewebsites.net";

export async function getSongs({ seed = 1, page = 1, pageSize = 10, locale ="en-US", likesAvg=5 } = {}) {
  const url = `${BASE_URL}/api/songs?seed=${seed}&page=${page}&pageSize=${pageSize}&locale=${locale}&likesAvg=${likesAvg}`;

  const res = await fetch(url);
  if (!res.ok) {
    throw new Error(`HTTP ${res.status}`);
  }

  return await res.json();
}
