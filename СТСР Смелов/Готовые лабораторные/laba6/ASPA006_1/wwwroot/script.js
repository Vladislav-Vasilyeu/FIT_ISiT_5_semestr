const cont = document.querySelector('.container');
const infodiv = document.querySelector('.info');
axios.get('https://localhost:7251/api/Celebrities')
    .then(cel => {
        for (let x of cel.data) {
            const img = document.createElement('img');
            img.src = `./photo/${x.reqPhotoPath}`;
            img.height = '230'
            cont.appendChild(img);
            img.addEventListener('click', (e) => {
                axios.get('https://localhost:7251/api/Lifeevents')
                    .then(info => {
                        infodiv.replaceChildren();
                        const hr1 = document.createElement('hr');
                        const hr2 = document.createElement('hr');
                        infodiv.appendChild(hr1);
                        for (let y of info.data) {
                            if (x.id == y.celebrityId) {
                                console.log(x);
                                console.log(y);
                                const text = document.createElement('p');
                                text.innerText = x.fullName + " " + y.date + " " + y.description;
                                infodiv.appendChild(text);
                            }
                        }
                        infodiv.appendChild(hr2);
                    })
                    .catch(error => {
                        console.error('Ошибка:', error);
                    });
            })
        }
    })
    .catch(error => {
        console.error('Ошибка:', error);
    });