SELECT wr.Id, wr.Name, wr.NeighborhoodId, ws.Id AS WalksId, ws.Date, ws.Duration, ws.WalkerId, ws.DogId
FROM Walker wr
LEFT JOIN Walks ws ON wr.Id = ws.WalkerId
WHERE wr.Id = 1