import {Container, Flex} from '@chakra-ui/react'
import type { NextPage } from 'next'

const Home: NextPage = () => {
  return (
      <Container maxWidth='container.xl' padding={0}>
          <Flex height='100vh' paddingY={20}>
              <h1>Hello Next.js</h1>
          </Flex>
      </Container>
  )
}

export default Home
