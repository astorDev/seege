import http from 'k6/http';

export const options = {
  vus: 100,
  duration: '30s',
};

const host = 'http://localhost:8402';

export default function () {
  const isHot = (__ITER % 4 === 0)
  let res = http.post(`${host}/exceptional-vegetables`, JSON.stringify({
    name: 'Potato',
    isHot
  }), { headers: { 'Content-Type': 'application/json' } });
}
