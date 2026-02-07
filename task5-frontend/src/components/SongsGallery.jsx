import { useEffect, useMemo, useRef, useState } from "react";
import { ThumbsUp } from "lucide-react";
import { getSongs } from "../api/songsApi";

export default function SongsGallery({ seed, locale, likesAvg, pageSize = 20 }) {
  const [gallerySongs, setGallerySongs] = useState([]);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState(null);

  const lastCardRef = useRef(null);
  const observerRef = useRef(null);

  const [cols, setCols] = useState(4);

  const ellipsis = useMemo(
    () => ({
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap",
    }),
    []
  );

  useEffect(() => {
    const calcCols = () => {
      const w = window.innerWidth;
      if (w < 640) return 1;
      if (w < 900) return 2;
      if (w < 1200) return 3;
      return 4;
    };

    const onResize = () => setCols(calcCols());
    onResize();

    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);

  useEffect(() => {
    setGallerySongs([]);
    setPage(1);
    setIsLoading(false);
    setHasMore(true);
    setError(null);
  }, [seed, locale, likesAvg]);

  useEffect(() => {
    if (!hasMore) return;

    setIsLoading(true);
    setError(null);

    getSongs({ seed, page, pageSize, locale, likesAvg })
      .then((data) => {
        setGallerySongs((prev) => [...prev, ...data.songs]);
        if (data.songs.length < pageSize) setHasMore(false);
      })
      .catch((err) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [seed, locale, likesAvg, page, pageSize, hasMore]);

  useEffect(() => {
    const lastElement = lastCardRef.current;
    if (!lastElement) return;

    if (observerRef.current) observerRef.current.disconnect();

    observerRef.current = new IntersectionObserver((entries) => {
      const entry = entries[0];
      if (entry.isIntersecting && !isLoading && hasMore) {
        setPage((p) => p + 1);
      }
    });

    observerRef.current.observe(lastElement);

    return () => observerRef.current?.disconnect();
  }, [gallerySongs, isLoading, hasMore]);

  return (
    <div style={{ paddingTop: 8 }}>
      <div
        style={{
          display: "grid",
          gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))`,
          gap: 14,
        }}
      >
        {gallerySongs.map((song, index) => {
          const isLast = index === gallerySongs.length - 1;

          const coverUrl = `/api/songs/${song.id}/cover?locale=${encodeURIComponent(locale)}`;
          
          return (
            <div
              key={song.id}
              ref={isLast ? lastCardRef : null}
              style={{
                background: "#fff",
                border: "1px solid #e6e8ec",
                borderRadius: 12,
                overflow: "hidden",
                boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
                transition: "transform 140ms ease, box-shadow 140ms ease",
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.transform = "translateY(-1px)";
                e.currentTarget.style.boxShadow = "0 6px 18px rgba(0,0,0,0.10)";
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = "translateY(0px)";
                e.currentTarget.style.boxShadow = "0 1px 2px rgba(0,0,0,0.06)";
              }}
            >
              <img
                src={coverUrl}
                alt={song.songTitle}
                style={{
                  width: "100%",
                  height: 360,
                  objectFit: "cover",
                  display: "block",
                }}
              />

              <div style={{ padding: "10px 12px" }}>
                <div
                  style={{
                    fontSize: 15,
                    fontWeight: 700,
                    color: "#111827",
                    ...ellipsis,
                  }}
                  title={song.songTitle}
                >
                  {song.songTitle}
                </div>

                <div
                  style={{
                    marginTop: 4,
                    fontSize: 13,
                    fontWeight: 600,
                    color: "#374151",
                    ...ellipsis,
                  }}
                  title={song.artist}
                >
                  {song.artist}
                </div>

                <div
                  style={{
                    marginTop: 8,
                    display: "flex",
                    alignItems: "center",
                    gap: 10,
                  }}
                >
                  <div
                    style={{
                      minWidth: 0,
                      flex: 1,
                      display: "flex",
                      alignItems: "center",
                      gap: 10,
                      fontSize: 12,
                      color: "#6b7280",
                    }}
                  >
                    <span style={{ ...ellipsis }} title={song.genre}>
                      {song.genre}
                    </span>

                    <span style={{ color: "#d1d5db" }}>•</span>

                    <span
                      style={{
                        ...ellipsis,
                        color: song.albumTitle === "Single" ? "#9ca3af" : "#6b7280",
                        fontWeight: song.albumTitle === "Single" ? 600 : 500,
                      }}
                      title={song.albumTitle}
                    >
                      {song.albumTitle}
                    </span>
                  </div>

                  <div
                    style={{
                      flex: "0 0 auto",
                      display: "inline-flex",
                      alignItems: "center",
                      gap: 6,
                      padding: "4px 10px",
                      borderRadius: 999,
                      background: "#2563eb",
                      color: "#fff",
                      fontSize: 12,
                      fontWeight: 600,
                      boxShadow: "0 1px 2px rgba(0,0,0,0.10)",
                      userSelect: "none",
                    }}
                    title="Likes"
                  >
                    <ThumbsUp size={14} />
                    <span>{song.likes ?? 0}</span>
                  </div>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <div style={{ padding: "14px 2px", color: "#6b7280", fontSize: 13 }}>
        {isLoading && <div>Loading...</div>}
        {error && <div style={{ color: "#dc2626" }}>{error}</div>}
        {!hasMore && !isLoading && <div>End of list</div>}
      </div>
    </div>
  );
}
