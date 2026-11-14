// scripts/duas-instancias-aula-07.js
// Duas instâncias da mesma API: o celular da Ana está ligado na da porta 5001 e assina as ordens dela.
// A ordem sai pela instância da porta 5000. O aviso chega no celular?
// Precisa das duas de pé: make run e make run PORTA=5001. Quem chama é o make duas-instancias.
import http from 'k6/http';
import { WebSocket } from 'k6/websockets';

export const options = { vus: 1, iterations: 1 };

const assinatura = 'subscription { ordemEnviada(usuarioId: 1) { id ativo valor } }';
const ordem = 'mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 1, preco: 1 }) { ordem { id } } }';

export default function () {
  let chegou = null;
  const ws = new WebSocket('ws://localhost:5001/graphql', 'graphql-transport-ws');
  ws.onopen = () => ws.send(JSON.stringify({ type: 'connection_init' }));
  ws.onmessage = (e) => {
    const m = JSON.parse(e.data);
    if (m.type === 'connection_ack') {
      ws.send(JSON.stringify({ id: '1', type: 'subscribe', payload: { query: assinatura } }));
      // A assinatura está feita. Agora a ordem sai pela outra instância.
      setTimeout(() => {
        const r = http.post('http://localhost:5000/graphql', JSON.stringify({ query: ordem }), { headers: { 'Content-Type': 'application/json' } });
        console.log(`Ordem enviada pela porta 5000: id ${r.json('data.enviarOrdem.ordem.id')}`);
      }, 2000);
    } else if (m.type === 'next') {
      chegou = m.payload.data.ordemEnviada;
    }
  };
  setTimeout(() => {
    console.log(chegou
      ? `O celular na porta 5001 recebeu a ordem ${chegou.id}.`
      : 'O celular na porta 5001 não recebeu nada em 6 segundos.');
    ws.close();
  }, 6000);
}
