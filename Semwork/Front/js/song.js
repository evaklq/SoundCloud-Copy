let usedSongIds = [];
document.addEventListener('DOMContentLoaded', async function () {
    await configPageSongs();
    const currentURL = new URL(window.location.href);
    const songId = currentURL.searchParams.get("id");
    const song = await getSongById(songId)
    
    const songNameElement = document.querySelector('.name');
    const songAuthorElement = document.querySelector('.author');
    const songIconElement = document.querySelector('.icon');
    const isLikeElement = document.querySelector('.like');

    songNameElement.textContent = song.Title;
    songAuthorElement.textContent = song.Artist;
    songIconElement.src = song.IconUrl;
    if (song.IsLike) {
        isLikeElement.src="/img/whiteHeart.png";
    } else {
        isLikeElement.src="/img/whiteUnfilled.png";
    }

    isLikeElement.addEventListener('click', function() {
        let currentSrc = isLikeElement.getAttribute('src');

        if (currentSrc && currentSrc.endsWith('/img/whiteHeart.png')) {
            changeLikeStatus(songId)
            isLikeElement.setAttribute('src', '/img/whiteUnfilled.png');
        } else {
            changeLikeStatus(songId)
            isLikeElement.setAttribute('src', '/img/whiteHeart.png');
        }
    });
    
    let playBtn = document.getElementById("playBtn");

    const wavesurfer = WaveSurfer.create({
        container: '#waveform',
        waveColor: '#4F4A85',
        progressColor: '#383351',
        responsive: true,
        height: 90,
        barRadius: 4
    });

    let songUrl = song.SongUrl;
    wavesurfer.load(songUrl);

    wavesurfer.on("ready", function () {
        playBtn.onclick = function () {
            wavesurfer.playPause();
            if (playBtn.src.includes("/img/play.png")) {
                playBtn.src = "/img/stop.png";
            } else {
                playBtn.src = "/img/play.png";
            }
        };
    });

    wavesurfer.on('finish', function () {
        playBtn.src = "/img/play.png";
        wavesurfer.stop();
    });
});

async function configPageSongs() {
    let emptySongs = document.querySelectorAll('.row');
    let readySongs = await getPopularSongs()

    emptySongs.forEach(function (song) {
        const currentSongIndex = getSongId();
        let currentSong = readySongs[currentSongIndex];
        const songNameElement = song.querySelector('.song-title');
        const songAuthorElement = song.querySelector('.artist');
        const songId = song.querySelector('.song');
        const songIconElement = song.querySelector('.iconSong');
        const isLikeElement = song.querySelector('.like-btn');

        songNameElement.textContent = currentSong.Title;
        songAuthorElement.textContent = currentSong.Artist;
        songId.id = currentSong.Id;
        songIconElement.src = currentSong.IconUrl;
        if (currentSong.IsLike) {
            isLikeElement.src="/img/purpleHert.png";
        } else {
            isLikeElement.src="/img/purpleUnfilled.png";
        }

        songId.addEventListener('click', function() {
            window.location.href = `/html/song.html?id=${songId.id}`;
        })

        isLikeElement.addEventListener('click', function() {
            let currentSrc = isLikeElement.getAttribute('src');

            if (currentSrc && currentSrc.endsWith('/img/purpleHert.png')) {
                changeLikeStatus(songId.id)
                isLikeElement.setAttribute('src', '/img/purpleUnfilled.png');
            } else {
                changeLikeStatus(songId.id)
                isLikeElement.setAttribute('src', '/img/purpleHert.png');
            }
        });
    })
}

async function getPopularSongs() {
    let getSongsUrl = "http://localhost:2400/get-main-songs"
    let optionsGet = {
        method: "GET",
        headers: {
            "Content-type": "application/json"
        },
    }
    try {
        const responseGet = await fetch(getSongsUrl, optionsGet)

        if (responseGet.ok) {
            return await responseGet.json();
        } else {
            alert("ошибка в получении песен" + responseGet.status)
            return null;
        }
    }
    catch (error) {
        alert("ошибка в ответе от получения песен" + error)
        return null;
    }
}

function getSongId() {
    const currentSongIndex = Math.floor(Math.random() * 13);
    if (!usedSongIds[currentSongIndex]) {
        usedSongIds[currentSongIndex] = currentSongIndex;
        return currentSongIndex;
    } else {
        return getSongId();
    }
}

async function getSongById(id) {
    let getSongUrl = "http://localhost:2400/get-song-by-id"
    let optionsGet = {
        method: "POST",
        headers: {
            "Content-type": "application/json"
        },
        body: JSON.stringify(id)
    }
    try {
        const songResponse = await fetch(getSongUrl, optionsGet)

        if (songResponse.ok) {
            return await songResponse.json();
        } else {
            alert("ошибка в получении песен" + songResponse.status)
            return null;
        }
    }
    catch (error) {
        alert("ошибка в ответе от получения песен" + error)
        return null;
    }
}

async function changeLikeStatus(id) {
    let changeLikeUrl = "http://localhost:2400/change-like"
    let options = {
        method: "POST",
        headers: {
            "Content-type": "application/json"
        },
        body: JSON.stringify(id)
    }
    try {
        const response = await fetch(changeLikeUrl, options)

        if (response.ok) {
        } else {
            alert("ошибка лайка" + response.status)
            return null;
        }
    }
    catch (error) {
        alert("ошибка лайка" + error)
        return null;
    }
}