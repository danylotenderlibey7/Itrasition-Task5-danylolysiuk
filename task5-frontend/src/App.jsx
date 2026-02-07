import { useEffect, useState } from "react";
import SongsTable from "./components/SongsTable.jsx";
import SongsGallery from "./components/SongsGallery.jsx";
import { getSongs } from "./api/songsApi";
import ControlsBar from "./components/ControlsBar.jsx";


function App() {
  const [songs, setSongs] = useState([]);
  const [error, setError] = useState(null);
  const [expandedId, setExpandedId] = useState(null);

  const [seed, setSeed] = useState(1);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [locale, setLocale] = useState("en-US");
  const [likesAvg, setLikesAvg] = useState(5);
  const [viewMode, setViewMode] = useState("table");

useEffect(() => {
    setError(null);

  getSongs({seed, page, pageSize, locale, likesAvg})
  .then(data => setSongs(data.songs))
  .catch(err=>{setError(err.message);
});
}, [seed, page, pageSize, locale, likesAvg]);

  return (
    <div style={{ padding: 20 }}>
      <h1>Task5 Songs</h1>
      <ControlsBar page={page} setPage={setPage} locale={locale} setLocale={setLocale} seed={seed} setSeed={setSeed} 
      likesAvg={likesAvg} setLikesAvg={setLikesAvg} setExpandedId={setExpandedId} 
      viewMode={viewMode} setViewMode={setViewMode}/>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {viewMode === "table" && (
          <SongsTable
            songs={songs}
            expandedId={expandedId}
            setExpandedId={setExpandedId}
            locale={locale}
          />
        )}

        {viewMode === "gallery" && (
          <SongsGallery locale={locale} seed={seed} likesAvg={likesAvg} pageSize={20}/>
        )}
    </div>
  );
}

export default App;
